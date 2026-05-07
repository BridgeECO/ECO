using UnityEngine;

public class PlayerHoverState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;
    private PlayerDataSO _data;

    private float _hoverTimer;

    public PlayerHoverState(PlayerStateMachine stateMachine, PlayerDataSO data)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;
        _data = data;
    }

    public void Enter()
    {
        _sm.HasUsedHover = true;
        _motor.SetVelocity(Vector2.zero);
        _hoverTimer = 0f;
        _input.OnDashReleased += HandleDashReleased;
    }

    public void Update()
    {
        _hoverTimer += Time.deltaTime;
        if (_hoverTimer < _data.MaxHoverTime)
        {
            return;
        }
        _sm.ChangeState(EPlayerState.Airborne);
    }

    public void Exit()
    {
        _input.OnDashReleased -= HandleDashReleased;
    }

    private void HandleDashReleased()
    {
        _sm.DashCooldownTimer = _data.DashCooldown;
        _sm.ChangeState(EPlayerState.Dash);
    }
}