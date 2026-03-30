using UnityEngine;

public class PlayerDashState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;

    private float _dashTimer;
    private float _dashDuration = 1f;
    private float _dashSpeed = 8f;
    private Vector2 _dashDirection;

    public PlayerDashState(PlayerStateMachine stateMachine)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;
    }

    public void Enter()
    {
        _dashTimer = 0f;
        Vector2 mouseWorldPos = _input.MouseWorldPosition;
        _dashDirection = (mouseWorldPos - (Vector2)_sm.transform.position).normalized;
        _sm.Motor.SetVelocity(_dashDirection * _dashSpeed);
    }

    public void Update()
    {
        _dashTimer += Time.deltaTime;
        _motor.SetVelocity(_dashDirection * _dashSpeed);

        if (_dashDuration <= _dashTimer)
        {
            _motor.SetVelocity(Vector2.zero);
            if (_sensor.IsGrounded)
            {
                _sm.ChangeState(EPlayerState.Grounded);
                return;
            }
            else
            {
                _sm.ChangeState(EPlayerState.Airborne);
                return;
            }
        }
    }

    public void Exit()
    {
    }
}