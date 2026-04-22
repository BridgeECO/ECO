using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDataSO", menuName = "Scriptable Objects/PlayerDataSO")]
public class PlayerDataSO : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("땅 위에서 좌우로 이동하는 속도. 높일수록 더 빠르게 이동합니다.")]
    [SerializeField]
    private float _groundMoveSpeed;

    [Tooltip("공중에서 좌우로 이동하는 속도. 현재는 지상과 동일하게 동작하므로 참고용 수치입니다.")]
    [SerializeField]
    private float _airMoveSpeed;

    [Tooltip("벽점프 직후처럼 이동 속도를 초과한 상태에서 정상 속도로 돌아오는 데 걸리는 시간을 결정합니다. 낮을수록 즉시 복귀하고, 높을수록 천천히 복귀합니다.")]
    [SerializeField]
    private float _airDeceleration;

    [Header("Jump & Gravity")]
    [Tooltip("점프 키를 끝까지 홀딩했을 때 도달하는 최대 높이 (블록 단위). 이 값이 포물선의 정점이 됩니다.")]
    [SerializeField]
    private float _jumpHeight;

    [Tooltip("점프 키를 끝까지 홀딩했을 때 정점에 도달하기까지 걸리는 시간 (초). 낮출수록 점프 전체 속도가 빨라지고 중력이 강해집니다. 높일수록 점프가 둥실둥실 느려집니다.")]
    [SerializeField]
    private float _maxJumpHoldTime;

    [Tooltip("정점을 찍고 낙하할 때 중력에 곱해지는 배율. 1이면 상승과 동일한 속도로 낙하, 높일수록 정점 이후 빠르게 떨어집니다.")]
    [SerializeField]
    private float _fallGravityMultiplier;

    [Tooltip("점프 중 키에서 손을 뗐을 때 추가로 적용되는 하강 가속 배율. 높일수록 손을 뗀 직후 더 빠르게 아래로 당겨집니다.")]
    [SerializeField]
    private float _earlyReleaseFallMultiplier;

    [Tooltip("점프 중 키에서 손을 뗐을 때 현재 상승 속도를 즉시 얼마나 줄일지 결정합니다. 0이면 즉시 정점, 1이면 속도 변화 없음. 0에 가까울수록 짧은 탭 점프가 더 명확해집니다.")]
    [SerializeField]
    private float _jumpCutMultiplier;

    [Tooltip("착지 직전에 점프 키를 미리 눌러도 착지 후 즉시 점프가 나가는 선입력 허용 시간 (초). 높일수록 여유롭게 점프 입력을 받아줍니다.")]
    [SerializeField]
    private float _jumpBufferTime;

    [Tooltip("낙하 시 도달할 수 있는 최대 속도 제한. 낮을수록 천천히, 높을수록 빠르게 떨어집니다. MaxJumpHoldTime을 낮췄을 때 너무 빨리 가속된다면 이 값을 함께 높여주세요.")]
    [SerializeField]
    private float _maxFallSpeed;

    [Tooltip("절벽 끝에서 발이 떨어진 후에도 허공에서 점프를 허용해주는 유예 시간 (초). 높일수록 절벽 끝에서 더 여유있게 점프할 수 있습니다.")]
    [SerializeField]
    private float _coyoteTime;

    [Tooltip("절벽 끝에서 발이 떨어진 후 수평 이동 거리가 이 값을 초과하면 코요테 점프가 불가합니다 (블록 단위). 너무 높으면 허공에서 점프하는 것처럼 보일 수 있습니다.")]
    [SerializeField]
    private float _coyoteDistance;

    [Header("Dash & Hover")]
    [Tooltip("대쉬 시 마우스 포인터 방향으로 날아가는 속도. 높일수록 더 빠르고 강렬한 대쉬가 됩니다.")]
    [SerializeField]
    private float _dashSpeed;

    [Tooltip("대쉬 후 날아가는 지속 시간 (초). 높일수록 더 멀리 날아갑니다.")]
    [SerializeField]
    private float _dashDuration;

    [Tooltip("마우스 좌클릭을 유지할 때 공중에서 제자리에 머무를 수 있는 최대 시간 (초). 0으로 설정하면 호버 기능이 사실상 비활성화됩니다.")]
    [SerializeField]
    private float _maxHoverTime;

    [Header("Wall")]
    [Tooltip("벽에 달라붙어 미끄러져 내려올 때의 하강 속도. 낮을수록 천천히 미끄러집니다.")]
    [SerializeField]
    private float _wallSlideSpeed;

    [Tooltip("벽점프 시 벽의 반대 방향(가로)으로 튀어나가는 힘. 높일수록 옆으로 더 멀리 날아갑니다.")]
    [SerializeField]
    private float _wallJumpPowerX;

    [Tooltip("벽점프 시 위로 솟구치는 힘. 높일수록 더 높이 뛰어오릅니다.")]
    [SerializeField]
    private float _wallJumpPowerY;

    [Tooltip("벽점프 직후 방향키 입력을 무시하는 시간 (초). 이 시간 동안 포물선 궤적이 강제 유지됩니다. 너무 낮으면 방향키로 궤적이 무너질 수 있습니다.")]
    [SerializeField]
    private float _wallJumpInputLockTime;

    [Header("Slip")]
    [Tooltip("플랫폼 모서리에 발이 아슬아슬하게 걸쳤을 때 허공에 뜨지 않도록 바닥으로 끌어내리는 속도. 너무 낮으면 모서리에 걸릴 수 있고, 너무 높으면 미끄러지는 느낌이 납니다.")]
    [SerializeField]
    private float _slipDownSpeed;

    public float GroundMoveSpeed { get => _groundMoveSpeed; }

    public float AirMoveSpeed { get => _airMoveSpeed; }

    public float AirDeceleration { get => _airDeceleration; }

    public float JumpHeight { get => _jumpHeight; }

    public float MaxJumpHoldTime { get => _maxJumpHoldTime; }

    public float Gravity { get => 2f * _jumpHeight / (_maxJumpHoldTime * _maxJumpHoldTime); }

    public float InitialJumpVelocity { get => 2f * _jumpHeight / _maxJumpHoldTime; }

    public float FallGravityMultiplier { get => _fallGravityMultiplier; }

    public float EarlyReleaseFallMultiplier { get => _earlyReleaseFallMultiplier; }

    public float JumpCutMultiplier { get => _jumpCutMultiplier; }

    public float MaxFallSpeed { get => _maxFallSpeed; }

    public float CoyoteTime { get => _coyoteTime; }

    public float CoyoteDistance { get => _coyoteDistance; }

    public float JumpBufferTime { get => _jumpBufferTime; }

    public float DashSpeed { get => _dashSpeed; }

    public float DashDuration { get => _dashDuration; }

    public float MaxHoverTime { get => _maxHoverTime; }

    public float WallSlideSpeed { get => _wallSlideSpeed; }

    public float WallJumpPowerX { get => _wallJumpPowerX; }

    public float WallJumpPowerY { get => _wallJumpPowerY; }

    public float WallJumpInputLockTime { get => _wallJumpInputLockTime; }

    public float SlipDownSpeed { get => _slipDownSpeed; }
}
