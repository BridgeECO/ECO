using UnityEngine;

public class PlayerDashState : IPlayerState
{
    private PlayerStateMachine _sm;
    private float _dashTimer;
    private float _dashDuration = 1f;
    private float _dashSpeed = 4f;
    private Vector2 _dashDirection;

    public PlayerDashState(PlayerStateMachine stateMachine)
    {
        _sm = stateMachine;
    }

    public void Enter()
    {
        _dashTimer = 0f;

        Vector2 mouseWorldPos = _sm.Input.MouseWorldPosition;
        _dashDirection = (mouseWorldPos - (Vector2)_sm.transform.position).normalized;

        _sm.Motor.SetVelocity(_dashDirection * _dashSpeed);
    }

    public void Update()
    {
        _dashTimer += Time.deltaTime;
        _sm.Motor.SetVelocity(_dashDirection * _dashSpeed);

        if (_dashTimer >= _dashDuration)
        {
            _sm.Motor.SetVelocity(Vector2.zero);
            _sm.ChangeState(EPlayerState.Airborne);
        }
    }

    public void Exit()
    {
    }
}