using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDataSO", menuName = "Scriptable Objects/PlayerDataSO")]
public class PlayerDataSO : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("지상에서의 좌우 이동 속도 (초당 이동하는 블록 수)")]
    [SerializeField]
    private float _groundMoveSpeed = 6f;
    public float GroundMoveSpeed { get => _groundMoveSpeed; }

    [Tooltip("공중에서의 좌우 이동 속도 (초당 이동하는 블록 수)")]
    [SerializeField]
    private float _airMoveSpeed = 2f;
    public float AirMoveSpeed { get => _airMoveSpeed; }

    [Tooltip("공중에서 최대 이동 속도를 초과했을 때(예: 벽점프 직후), 원래 속도로 부드럽게 돌아오기 위한 감속도")]
    [SerializeField]
    private float _airDeceleration = 15f;
    public float AirDeceleration { get => _airDeceleration; }

    [Header("Jump & Gravity")]
    [Tooltip("점프를 누르는 순간 위로 솟구치는 초기 상승 속도")]
    [SerializeField]
    private float _jumpSpeed = 5.33f;
    public float JumpSpeed { get => _jumpSpeed; }

    [Tooltip("점프 키를 누르고 있을 때 중력을 무시하고 일직선으로 상승하는 최대 시간 (초)")]
    [SerializeField]
    private float _maxJumpHoldTime = 1.5f;
    public float MaxJumpHoldTime { get => _maxJumpHoldTime; }

    [Tooltip("공중에 있을 때 매 초마다 아래 방향으로 더해지는 중력 가속도")]
    [SerializeField]
    private float _gravity = 20f;
    public float Gravity { get => _gravity; }

    [Tooltip("공중에서 떨어질 때 도달할 수 있는 최대 낙하 속도 제한")]
    [SerializeField]
    private float _maxFallSpeed = 5.33f;
    public float MaxFallSpeed { get => _maxFallSpeed; }

    [Tooltip("절벽에서 발이 떨어진 후에도 허공에서 점프를 허용해주는 유예 시간 (초)")]
    [SerializeField]
    private float _coyoteTime = 0.5f;
    public float CoyoteTime { get => _coyoteTime; }

    [Tooltip("절벽에서 발이 떨어진 후에도 허공에서 점프를 허용해주는 최대 이탈 거리 (블록 수)")]
    [SerializeField]
    private float _coyoteDistance = 0.2f;
    public float CoyoteDistance { get => _coyoteDistance; }

    [Tooltip("공중에서 착지하기 직전에 점프 키를 눌러도 착지 즉시 점프가 나가도록 기억하는 선입력 시간 (초)")]
    [SerializeField]
    private float _jumpBufferTime = 0.2f;
    public float JumpBufferTime { get => _jumpBufferTime; }

    [Header("Dash & Hover")]
    [Tooltip("대쉬 시 마우스 포인터 방향으로 날아가는 이동 속도")]
    [SerializeField]
    private float _dashSpeed = 8f;
    public float DashSpeed { get => _dashSpeed; }

    [Tooltip("대쉬 상태가 유지되며 날아가는 시간 (초)")]
    [SerializeField]
    private float _dashDuration = 1f;
    public float DashDuration { get => _dashDuration; }

    [Tooltip("공중 대쉬를 위해 마우스 좌클릭을 유지할 때 제자리에 머무를 수 있는 최대 체공 시간 (초)")]
    [SerializeField]
    private float _maxHoverTime = 1f;
    public float MaxHoverTime { get => _maxHoverTime; }

    [Header("Wall")]
    [Tooltip("벽에 매달려서 아래로 미끄러져 내려올 때의 하강 속도")]
    [SerializeField]
    private float _wallSlideSpeed = 2.66f;
    public float WallSlideSpeed { get => _wallSlideSpeed; }

    [Tooltip("벽 점프 시, 벽의 반대 방향(X축)으로 튕겨 나가는 힘의 크기")]
    [SerializeField]
    private float _wallJumpPowerX = 10f;
    public float WallJumpPowerX { get => _wallJumpPowerX; }

    [Tooltip("벽 점프 시, 위쪽(Y축)으로 솟구치는 힘의 크기")]
    [SerializeField]
    private float _wallJumpPowerY = 10f;
    public float WallJumpPowerY { get => _wallJumpPowerY; }

    [Tooltip("벽 점프 직후 45도 포물선 궤적을 보장하기 위해 플레이어의 좌우 방향키 입력을 강제로 막는 시간 (초)")]
    [SerializeField]
    private float _wallJumpInputLockTime = 0.2f;
    public float WallJumpInputLockTime { get => _wallJumpInputLockTime; }

    [Header("Slip")]
    [Tooltip("지형이나 플랫폼 모서리에 아슬아슬하게 발이 걸쳤을 때, 허공에 둥둥 뜨지 않도록 바닥으로 강제로 끌어내리는 속도")]
    [SerializeField]
    private float _slipDownSpeed = 15f;
    public float SlipDownSpeed { get => _slipDownSpeed; }
}