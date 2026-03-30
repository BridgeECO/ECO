using UnityEngine;

public class PlayerHoverState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;

    private float _hoverTimer;
    private float _maxHoverTime = 1f;

    public PlayerHoverState(PlayerStateMachine stateMachine)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;
    }

    public void Enter()
    {
        _sm.HasUsedHover = true;
        _input.OnDashReleased += HandleDashReleased;
        _motor.SetVelocity(Vector2.zero);
        _hoverTimer = 0f;
    }

    public void Update()
    {
        _motor.SetVelocity(Vector2.zero);
        _hoverTimer += Time.deltaTime;
        if (_maxHoverTime <= _hoverTimer)
        {
            _sm.ChangeState(EPlayerState.Airborne);
        }
    }

    public void Exit()
    {
        _input.OnDashReleased -= HandleDashReleased;
    }

    private void HandleDashReleased()
    {
        _sm.ChangeState(EPlayerState.Dash);
    }
}