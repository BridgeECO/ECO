using UnityEngine;

public class PlayerWallSlideState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;

    private float _wallSlideSpeed;
    private float _wallJumpPowerX;
    private float _wallJumpPowerY;

    public PlayerWallSlideState(PlayerStateMachine stateMachine, PlayerDataSO data)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;

        _wallSlideSpeed = data.WallSlideSpeed;
        _wallJumpPowerX = data.WallJumpPowerX;
        _wallJumpPowerY = data.WallJumpPowerY;
    }

    public void Enter()
    {
        _motor.SetVelocity(new Vector2(0f, -_wallSlideSpeed));
        _input.OnJumpPressed += HandleWallJump;
    }

    public void Update()
    {
        float xInput = _input.HorizontalInput;
        _motor.SetVelocityY(-_wallSlideSpeed);

        if (_sensor.IsGrounded)
        {
            _sm.ChangeState(EPlayerState.Grounded);
            return;
        }

        if (!_sensor.IsBodyTouching || xInput != _sensor.WallDirection)
        {
            _sm.ChangeState(EPlayerState.Airborne);
            return;
        }
    }

    public void Exit()
    {
        _input.OnJumpPressed -= HandleWallJump;
    }

    private void HandleWallJump()
    {
        _sm.JumpBufferTimer = 0f;
        float jumpDir = -_sensor.WallDirection;
        _motor.SetVelocity(new Vector2(jumpDir * _wallJumpPowerX, _wallJumpPowerY));
        _sm.ChangeState(EPlayerState.Airborne);
    }
}