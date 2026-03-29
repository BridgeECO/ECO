using UnityEngine;

public class PlayerHoverState : IPlayerState
{
    private PlayerStateMachine _sm;
    private float _hoverTimer;
    private float _maxHoverTime = 1f;

    public PlayerHoverState(PlayerStateMachine stateMachine)
    {
        _sm = stateMachine;
    }

    public void Enter()
    {
        _sm.HasUsedHover = true;
        _sm.Motor.SetVelocity(Vector2.zero);
        _hoverTimer = 0f;

        _sm.Input.OnDashReleased += HandleDashReleased;
    }

    public void Update()
    {
        _sm.Motor.SetVelocity(Vector2.zero);

        _hoverTimer += Time.deltaTime;
        if (_maxHoverTime <= _hoverTimer)
        {
            _sm.ChangeState(EPlayerState.Airborne);
        }
    }

    public void Exit()
    {
        _sm.Input.OnDashReleased -= HandleDashReleased;
    }

    private void HandleDashReleased()
    {
        _sm.ChangeState(EPlayerState.Dash);
    }
}