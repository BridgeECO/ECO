using UnityEngine;

public class PlayerGroundedState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;

    public PlayerGroundedState(PlayerStateMachine stateMachine)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;
    }

    public void Enter()
    {
        _sm.HasUsedHover = false;
        _sm.CoyoteTimer = 0.5f;
        _motor.SetVelocityY(0f);
        _input.OnDashPressed += HandleDashPressed;
    }

    public void Update()
    {
        _sm.CoyoteTimer = 0.5f;
        float xInput = _input.HorizontalInput;
        _motor.SetVelocityX(xInput * 6f);

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
        _sm.ChangeState(EPlayerState.Dash);
    }
}