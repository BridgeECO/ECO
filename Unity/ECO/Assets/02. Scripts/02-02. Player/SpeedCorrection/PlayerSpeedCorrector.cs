using UnityEngine;

/// <summary>
/// 외부 요인이 거는 추가 속도 보정을 모아 PlayerMotor의 보정 채널에 실어준다.
/// 가로는 지상에서 실시간으로 따라가고, 공중에서는 마지막 값을 물고 있다가 반대 입력에만 감쇄시킨다.
/// 세로는 벽타기(벨트 옆면에 붙음) 동안만 싣고, 떠나는 순간 없어진다.
/// </summary>
public class PlayerSpeedCorrector : MonoBehaviour
{
    private PlayerMotor _motor;
    private PlayerInput _input;
    private PlayerDataSO _playerData;
    private PlayerSpeedCorrectionRegistry _registry = new PlayerSpeedCorrectionRegistry();
    private float _correction;
    private float _correctionY;

    public float Correction => _correction;
    public float CorrectionY => _correctionY;
    public int CorrectionCount => _registry.CorrectionCount;

    private void Awake()
    {
        _motor = GetComponent<PlayerMotor>();
        _input = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        _motor.OnTeleported += ClearCorrections;
        _input.OnJumpPressed += HandleJumpPressed;
    }

    private void OnDisable()
    {
        _motor.OnTeleported -= ClearCorrections;
        _input.OnJumpPressed -= HandleJumpPressed;
        ClearCorrections();
    }

    public void InitPlayerData(PlayerDataSO playerData)
    {
        _playerData = playerData;
    }

    public void SetCorrection(int sourceId, float magnitude, ESpeedCorrectionDirection direction)
    {
        _registry.SetCorrection(sourceId, magnitude, direction);
    }

    public void RemoveCorrection(int sourceId)
    {
        _registry.RemoveCorrection(sourceId);
    }

    public void ClearCorrections()
    {
        _registry.ClearCorrections();
        _correction = 0f;
        _correctionY = 0f;

        // 리스폰 텔레포트는 프레임 중간에 일어나므로, 다음 Refresh를 기다리지 않고 모터를 즉시 비운다.
        if (_motor != null)
        {
            _motor.SetSpeedCorrectionX(0f);
            _motor.SetSpeedCorrectionY(0f);
        }
    }

    public void Refresh(EPlayerState playerState, float deltaTime)
    {
        float liveSum = _registry.ResolveSum(_input.HorizontalInput);
        _correctionY = 0f;

        if (playerState == EPlayerState.Grounded)
        {
            _correction = liveSum;
        }
        else if (playerState == EPlayerState.Airborne)
        {
            _correction = RefreshAirborne(liveSum, deltaTime);
        }
        else if (playerState == EPlayerState.WallSlide)
        {
            // 벽타기는 가로를 스스로 0으로 강제하므로 가로는 싣지 않는다. 세로는 벨트 옆면이 건 값을 미끄러짐에 더한다.
            _correction = 0f;
            _correctionY = RefreshWallSlideVertical();
        }
        else
        {
            // 체류/대쉬는 속도를 스스로 강제하는 상태다. 보정을 남겨두면 조준한 대쉬 궤적이 비틀린다.
            _correction = 0f;
        }

        _motor.SetSpeedCorrectionX(_correction);
        _motor.SetSpeedCorrectionY(_correctionY);
    }

    // 공중에서는 한 번 실린 보정이 저절로 사라지지 않는다. 접촉이 끊겨도 값을 그대로 물고 있다가
    // 반대 방향 입력을 받을 때만 감쇄시킨다.
    private float RefreshAirborne(float liveSum, float deltaTime)
    {
        if (liveSum != 0f)
        {
            return liveSum;
        }

        // 양방향 보정에 무입력이면 합이 0이 되는데, 소스가 아직 붙어 있으므로 감쇄 대상이 아니다.
        if (0 < _registry.HorizontalCorrectionCount)
        {
            return _correction;
        }

        return Attenuate(_correction, _input.HorizontalInput, deltaTime);
    }

    // 벽점프는 PlayerInput.Update의 점프 이벤트 안에서 공중으로 바뀌는데, 그 Update가 PlayerStateMachine.Update(Refresh)보다
    // 늦게 돌면 다음 Refresh 전 물리 스텝에 벽타기 세로 보정이 한 번 실린다. 세로를 싣는 상태(벽타기)에서 점프는 곧 옆면을
    // 떠나는 것이라 그 자리에서 내린다. 다른 상태는 세로가 이미 0이라 바뀌는 것이 없다.
    private void HandleJumpPressed()
    {
        _correctionY = 0f;
        _motor.SetSpeedCorrectionY(0f);
    }

    // 입력 없이 오른쪽 벽에 닿으면 벽타기에 들어갔다 다음 프레임에 나오는데(PlayerAirborneState의 Mathf.Sign(0f) == 1),
    // 그 프레임마다 세로를 실으면 위로 조금씩 밀리므로 벽 쪽 입력이 있을 때만 싣는다.
    private float RefreshWallSlideVertical()
    {
        if (_input.HorizontalInput == 0f)
        {
            return 0f;
        }
        return _registry.ResolveVerticalSum();
    }

    private float Attenuate(float correction, float horizontalInput, float deltaTime)
    {
        if (0f <= horizontalInput * correction)
        {
            return correction;
        }

        // MoveTowards가 0에서 멈추므로 잔류 보정이 반대 부호로 넘어가지 않는다.
        return Mathf.MoveTowards(correction, 0f, _playerData.AirCorrectionDecayRate * deltaTime);
    }
}
