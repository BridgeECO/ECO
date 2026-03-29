using UnityEngine;

public class PlayerGroundedState : IPlayerState
{
    private PlayerStateMachine _sm;

    private PlayerSensor _sensor;
    private PlayerMotor _motor;

    public PlayerGroundedState(PlayerStateMachine stateMachine)
    {
        _sm = stateMachine;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;
    }

    public void Enter()
    {
        _sm.HasUsedHover = false;
        _sm.CoyoteTimer = 0.5f;
        _motor.SetVelocityY(0f);
    }

    public void Update()
    {
        _sm.CoyoteTimer = 0.5f;

        float xInput = _sm.Input.HorizontalInput;
        _motor.SetVelocityX(xInput * 6f);

        if (0f < _sm.JumpBufferTimer)
        {
            _sm.JumpBufferTimer = 0f;
            _sm.ChangeState(EPlayerState.Airborne);
            return;
        }

        if (!_sensor.IsGrounded)
        {
            _sm.ChangeState(EPlayerState.Airborne);
        }
    }

    public void Exit() 
    { 
    }
}