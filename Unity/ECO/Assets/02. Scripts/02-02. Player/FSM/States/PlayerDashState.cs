using UnityEngine;

public class PlayerDashState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;

    private float _dashSpeed;
    private float _dashDuration;

    private float _dashTimer;
    private Vector2 _dashDirection;

    public PlayerDashState(PlayerStateMachine stateMachine, PlayerDataSO data)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;

        _dashSpeed = data.DashSpeed;
        _dashDuration = data.DashDuration;
    }

    public void Enter()
    {
        _dashTimer = 0f;

        Vector2 mouseWorldPos = _input.MouseWorldPosition;
        _dashDirection = (mouseWorldPos - (Vector2)_sm.transform.position).normalized;

        _motor.SetVelocity(_dashDirection * _dashSpeed);
    }

    public void Update()
    {
        _dashTimer += Time.deltaTime;
        _motor.SetVelocity(_dashDirection * _dashSpeed);
        if (_dashTimer < _dashDuration)
        {
            return;
        }
        _motor.SetVelocity(Vector2.zero);

        if (_sensor.IsGrounded)
        {
            _sm.ChangeState(EPlayerState.Grounded);
            return;
        }
        _sm.ChangeState(EPlayerState.Airborne);
    }

    public void Exit()
    {
    }
}