using UnityEngine;

public class PlayerWallSlideState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;
    private PlayerDataSO _data;

    public PlayerWallSlideState(PlayerStateMachine stateMachine, PlayerDataSO data)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;
        _data = data;
    }

    public void Enter()
    {
        _motor.SetVelocity(new Vector2(0f, -_data.WallSlideSpeed));
        _input.OnJumpPressed += HandleWallJump;
    }

    public void Update()
    {
        float xInput = _input.HorizontalInput;
        _motor.SetVelocityY(-_data.WallSlideSpeed);

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
        _sm.InputLockTimer = _data.WallJumpInputLockTime;
        _sm.LastWallJumpDir = _sensor.WallDirection;

        float jumpDir = -_sensor.WallDirection;
        _motor.SetVelocity(new Vector2(jumpDir * _data.WallJumpPowerX, _data.WallJumpPowerY));
        _sm.ChangeState(EPlayerState.Airborne);
    }
}