using UnityEngine;

public class PlayerGroundedState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;

    private float _coyoteTime;
    private float _groundMoveSpeed;

    public PlayerGroundedState(PlayerStateMachine stateMachine, PlayerDataSO data)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;

        _coyoteTime = data.CoyoteTime;
        _groundMoveSpeed = data.GroundMoveSpeed;
    }

    public void Enter()
    {
        _sm.HasUsedHover = false;
        _sm.CoyoteTimer = _coyoteTime;
        _motor.SetVelocityY(0f);

        _input.OnDashPressed += HandleDashPressed;
    }

    public void Update()
    {
        _sm.CoyoteTimer = _coyoteTime;

        float xInput = _input.HorizontalInput;
        _motor.SetVelocityX(xInput * _groundMoveSpeed);

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