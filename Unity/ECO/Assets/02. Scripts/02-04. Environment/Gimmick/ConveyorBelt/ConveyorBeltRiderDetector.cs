using UnityEngine;

/// <summary>
/// 벨트 지형 위에 선 플레이어를 추적하고, 벨트 위에 있는 동안 벨트 방향 추가 속도 보정을 건다.
/// 발이 윗면에 닿았을 때뿐 아니라, 지상 상태인 플레이어의 접지 판정이 이 벨트만 지면으로 보는 동안도 벨트 위로 본다.
/// 기믹이 런타임에 붙이고 InitBelt로 회전 방향과 세기를 넘긴다.
/// </summary>
public class ConveyorBeltRiderDetector : MonoBehaviour
{
    private Collider2D _beltCollider;
    private ConveyorBeltSurfaceResolver _surfaceResolver;
    private float _speed;
    private bool _isBeltActive;
    private int _sourceId;

    private Collider2D _riderFeet;
    private PlayerSpeedCorrector _riderCorrector;
    private PlayerStateMachine _riderStateMachine;
    private bool _isCorrectionApplied;
    private bool _isContactStale;

    private void Awake()
    {
        _beltCollider = GetComponent<Collider2D>();
        _sourceId = GetInstanceID();
    }

    // OnCollisionStay2D는 충돌 콜백 재사용이 꺼져 있어 스텝마다 Collision2D를 할당하므로, 타고 내릴 때만 콜백을 받고
    // 닿은 면은 할당 없는 접촉 조회로 스텝마다 다시 본다. 둥근 모서리에 먼저 닿았다가 윗면으로 올라선 경우도 이렇게 잡힌다.
    private void FixedUpdate()
    {
        RefreshCorrection();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 몸통·미끄러짐 콜라이더가 옆면에 닿은 것은 탑승이 아니다. 이미 추적 중인 발이 다시 닿은 것도 무시한다.
        if (_riderFeet == null && collision.collider.CompareTag(nameof(ETags.PlayerFeet))
            && TryGetRiderStateMachine(collision.collider, out PlayerStateMachine stateMachine))
        {
            TrackRider(collision.collider, stateMachine);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // 발이 떨어져도 접지 판정이 아직 이 벨트 위로 보면 탑승이 이어지므로, 그때 놓는 일은 FixedUpdate가 맡는다.
        if (collision.collider != _riderFeet || IsStandingWithoutContact(_riderFeet, _riderStateMachine, out _))
        {
            return;
        }
        ReleaseRider();
    }

    private void OnDisable()
    {
        ReleaseRider();
    }

    public void InitBelt(EConveyorBeltDirection direction, float speed)
    {
        _surfaceResolver = new ConveyorBeltSurfaceResolver(direction);
        _speed = speed;
    }

    public void SetBeltActive(bool isActive)
    {
        _isBeltActive = isActive;
        if (!isActive)
        {
            RemoveCorrection();
        }
    }

    // 플레이어가 먼저 파괴됐으면 걷을 보정도 없다.
    public void RemoveCorrection()
    {
        if (_riderCorrector != null)
        {
            _riderCorrector.RemoveCorrection(_sourceId);
        }
        _isCorrectionApplied = false;
    }

    private void RefreshCorrection()
    {
        if (_riderFeet == null && !TryTrackFloatingRider())
        {
            return;
        }

        // 텔레포트는 강체 위치만 옮겨, 다음 물리 단계 전까지 접촉 정보가 옛 자리 것이다. 한 단계 쉬어 옛 자리 보정을 다시 걸지 않는다.
        if (_isContactStale)
        {
            _isContactStale = false;
            return;
        }

        // 사망부터 부활까지는 밀지 않는다(PlayerCrushDetector와 같은 구간).
        if (!_isBeltActive || IsRespawning())
        {
            RemoveCorrection();
            return;
        }

        if (_surfaceResolver.TryResolveCorrection(_riderFeet, _beltCollider, out ESpeedCorrectionDirection direction))
        {
            ApplyCorrection(direction);
            return;
        }

        if (IsStandingWithoutContact(_riderFeet, _riderStateMachine, out bool isBeltBelow))
        {
            ApplyCorrection(_surfaceResolver.ResolveTopSurfaceDirection());
            return;
        }

        RemoveCorrection();
        if (!isBeltBelow && !_riderFeet.IsTouching(_beltCollider))
        {
            ReleaseRider();
        }
    }

    // 발이 윗면에 닿지 않았어도 지상 상태이고 다른 지면 위가 아니면 이 벨트 위로 본다. ① 지상 상태는 y 속도가 0이라 옆 지면보다
    // 0.1보다 얕게 낮은 벨트로 걸어 들어오면 떠 있는데, 접지 판정은 발 아래의 이 벨트를 지면으로 본다. ② 끝에서 떨어질 때 접지 판정은
    // 한두 프레임 늦게 풀리므로, 이미 건 보정은 상태가 공중으로 바뀔 때까지 남겨야(그 사이 발 아래에 벨트가 없어도) 공중으로 넘어간다.
    private bool IsStandingWithoutContact(Collider2D feet, PlayerStateMachine stateMachine, out bool isBeltBelow)
    {
        _surfaceResolver.ScanGroundBelow(feet, _beltCollider, out isBeltBelow, out bool isOnOtherSupport);
        return stateMachine.CurrentPlayerState == EPlayerState.Grounded
            && !isOnOtherSupport
            && (isBeltBelow || _isCorrectionApplied);
    }

    // 떠 있는 탑승자에게는 충돌 콜백이 오지 않으므로, 추적 중인 탑승자가 없을 때 윗면 바로 위에 들어온 발을 찾아 잡는다.
    private bool TryTrackFloatingRider()
    {
        if (!_isBeltActive || IsRespawning() || !_surfaceResolver.TryFindFeetAbove(_beltCollider, out Collider2D feet))
        {
            return false;
        }

        if (!TryGetRiderStateMachine(feet, out PlayerStateMachine stateMachine) || !IsStandingWithoutContact(feet, stateMachine, out _))
        {
            return false;
        }

        TrackRider(feet, stateMachine);
        return true;
    }

    private bool TryGetRiderStateMachine(Collider2D feet, out PlayerStateMachine stateMachine)
    {
        stateMachine = null;
        Rigidbody2D riderRigidbody = feet.attachedRigidbody;
        return riderRigidbody != null && riderRigidbody.TryGetComponent(out stateMachine);
    }

    private void TrackRider(Collider2D feet, PlayerStateMachine stateMachine)
    {
        _riderFeet = feet;
        _riderStateMachine = stateMachine;
        _riderCorrector = stateMachine.SpeedCorrector;
        _riderStateMachine.Motor.OnTeleported += HandleRiderTeleported;
    }

    private void ApplyCorrection(ESpeedCorrectionDirection direction)
    {
        _riderCorrector.SetCorrection(_sourceId, _speed, direction);
        _isCorrectionApplied = true;
    }

    private void ReleaseRider()
    {
        RemoveCorrection();
        if (_riderStateMachine != null)
        {
            _riderStateMachine.Motor.OnTeleported -= HandleRiderTeleported;
        }
        _riderFeet = null;
        _riderCorrector = null;
        _riderStateMachine = null;
        _isContactStale = false;
    }

    private void HandleRiderTeleported()
    {
        RemoveCorrection();
        _isContactStale = true;
    }

    private bool IsRespawning()
    {
        return RespawnManager.HasInstance && RespawnManager.Instance.IsRespawning;
    }
}
