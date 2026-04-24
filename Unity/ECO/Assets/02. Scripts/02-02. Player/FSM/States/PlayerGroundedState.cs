using UnityEngine;

public class PlayerGroundedState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;
    private PlayerDataSO _data;

    public PlayerGroundedState(PlayerStateMachine stateMachine, PlayerDataSO data)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;
        _data = data;
    }

    public void Enter()
    {
        _sm.HasUsedHover = false;
        _sm.CoyoteTimer = _data.CoyoteTime;
        _sm.LastWallJumpDir = 0f;
        _motor.SetVelocityY(0f);

        _input.OnDashPressed += HandleDashPressed;
    }

    public void Update()
    {
        _sm.CoyoteTimer = _data.CoyoteTime;

        float xInput = _input.HorizontalInput;
        _motor.SetVelocityX(xInput * _data.GroundMoveSpeed);

        if (_sm.JumpBufferTimer > 0f)
        {
            _sm.ChangeState(EPlayerState.Airborne);
            return;
        }

        if (!_sensor.IsGrounded)
        {
            _sm.ChangeState(EPlayerState.Airborne);
            return;
        }
    }

    public void Exit()
    {
        _input.OnDashPressed -= HandleDashPressed;
    }

    private void HandleDashPressed()
    {
        if (_sm.HasUsedHover)
        {
            return;
        }
        if (0f < _sm.DashCooldownTimer)
        {
            return;
        }
        _sm.ChangeState(EPlayerState.Hover);
    }
}