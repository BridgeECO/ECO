using UnityEngine;

/// <summary>
/// 외부 요인이 거는 추가 속도 보정을 모아 PlayerMotor의 보정 채널에 실어준다.
/// 지상에서는 실시간으로 따라가고, 공중에서는 마지막 값을 물고 있다가 반대 입력에만 감쇄시킨다.
/// </summary>
public class PlayerSpeedCorrector : MonoBehaviour
{
    private PlayerMotor _motor;
    private PlayerInput _input;
    private PlayerDataSO _playerData;
    private PlayerSpeedCorrectionRegistry _registry = new PlayerSpeedCorrectionRegistry();
    private float _correction;

    public float Correction => _correction;
    public int CorrectionCount => _registry.CorrectionCount;

    private void Awake()
    {
        _motor = GetComponent<PlayerMotor>();
        _input = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        _motor.OnTeleported += ClearCorrections;
    }

    private void OnDisable()
    {
        _motor.OnTeleported -= ClearCorrections;
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

        // 리스폰 텔레포트는 프레임 중간에 일어나므로, 다음 Refresh를 기다리지 않고 모터를 즉시 비운다.
        if (_motor != null)
        {
            _motor.SetSpeedCorrectionX(0f);
        }
    }

    public void Refresh(EPlayerState playerState, float deltaTime)
    {
        float liveSum = _registry.ResolveSum(_input.HorizontalInput);

        if (playerState == EPlayerState.Grounded)
        {
            _correction = liveSum;
        }
        else if (playerState == EPlayerState.Airborne)
        {
            _correction = RefreshAirborne(liveSum, deltaTime);
        }
        else
        {
            // 벽타기/체류/대쉬는 속도를 스스로 강제하는 상태다. 보정을 남겨두면 벽에서 밀려나거나
            // 조준한 대쉬 궤적이 비틀린다.
            _correction = 0f;
        }

        _motor.SetSpeedCorrectionX(_correction);
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
        if (0 < _registry.CorrectionCount)
        {
            return _correction;
        }

        return Attenuate(_correction, _input.HorizontalInput, deltaTime);
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
