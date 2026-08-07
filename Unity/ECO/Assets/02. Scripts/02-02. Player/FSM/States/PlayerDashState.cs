using UnityEngine;

public class PlayerDashState : IPlayerState
{
    private IPlayerFsmContext _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;
    private PlayerDataSO _data;

    private float _dashTimer;
    private Vector2 _dashDirection;

    public PlayerDashState(IPlayerFsmContext stateMachine, PlayerDataSO data)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;
        _data = data;
    }

    public void Enter()
    {
        _dashTimer = 0f;

        Vector2 mouseWorldPos = _input.MouseWorldPosition;
        _dashDirection = (mouseWorldPos - (Vector2)_sm.Transform.position).normalized;

        _motor.SetVelocity(_dashDirection * _data.DashSpeed);
    }

    public void Update()
    {
        _dashTimer += Time.deltaTime;
        _motor.SetVelocity(_dashDirection * _data.DashSpeed);
        if (_dashTimer < _data.DashDuration)
        {
            return;
        }
        _motor.SetVelocity(Vector2.zero);

        if (_sensor.IsOnGround)
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