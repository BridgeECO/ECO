using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDataSO", menuName = "Scriptable Objects/PlayerDataSO")]
public class PlayerDataSO : ScriptableObject
{
    [Header("Movement")]
    [SerializeField]
    private float _groundMoveSpeed = 6f;
    public float GroundMoveSpeed { get => _groundMoveSpeed; }

    [SerializeField]
    private float _airMoveSpeed = 2f;
    public float AirMoveSpeed { get => _airMoveSpeed; }

    [Header("Jump & Gravity")]
    [SerializeField]
    private float _jumpSpeed = 5.33f;
    public float JumpSpeed { get => _jumpSpeed; }

    [SerializeField]
    private float _maxJumpHoldTime = 1.5f;
    public float MaxJumpHoldTime { get => _maxJumpHoldTime; }

    [SerializeField]
    private float _gravity = 20f;
    public float Gravity { get => _gravity; }

    [SerializeField]
    private float _maxFallSpeed = 5.33f;
    public float MaxFallSpeed { get => _maxFallSpeed; }

    [SerializeField]
    private float _coyoteTime = 0.5f;
    public float CoyoteTime { get => _coyoteTime; }

    [SerializeField]
    private float _coyoteDistance = 0.2f;
    public float CoyoteDistance { get => _coyoteDistance; }

    [SerializeField]
    private float _jumpBufferTime = 0.2f;
    public float JumpBufferTime { get => _jumpBufferTime; }

    [Header("Dash & Hover")]
    [SerializeField]
    private float _dashSpeed = 8f;
    public float DashSpeed { get => _dashSpeed; }

    [SerializeField]
    private float _dashDuration = 1f;
    public float DashDuration { get => _dashDuration; }

    [SerializeField]
    private float _maxHoverTime = 1f;
    public float MaxHoverTime { get => _maxHoverTime; }

    [Header("Wall")]
    [SerializeField]
    private float _wallSlideSpeed = 2.66f;
    public float WallSlideSpeed { get => _wallSlideSpeed; }

    [SerializeField]
    private float _wallJumpPowerX = 10f;
    public float WallJumpPowerX { get => _wallJumpPowerX; }

    [SerializeField]
    private float _wallJumpPowerY = 10f;
    public float WallJumpPowerY { get => _wallJumpPowerY; }

    [SerializeField]
    private float _wallJumpInputLockTime = 0.2f;
    public float WallJumpInputLockTime { get => _wallJumpInputLockTime; }

    [Header("Slip")]
    [SerializeField]
    private float _slipDownSpeed = 15f;
    public float SlipDownSpeed { get => _slipDownSpeed; }
}