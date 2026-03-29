using UnityEngine;

public class PlayerWallSlideState : IPlayerState
{
    private PlayerStateMachine _sm;
    private float _wallSlideSpeed = 2.66f;

    public PlayerWallSlideState(PlayerStateMachine stateMachine)
    {
        _sm = stateMachine;
    }

    public void Enter()
    {
        _sm.Motor.SetVelocity(new Vector2(0f, -_wallSlideSpeed));
        _sm.Input.OnJumpPressed += HandleWallJump;
    }

    public void Update()
    {
        _sm.Motor.SetVelocityY(-_wallSlideSpeed);

        float xInput = _sm.Input.HorizontalInput;

        if (_sm.Sensor.IsGrounded)
        {
            _sm.ChangeState(EPlayerState.Grounded);
            return;
        }

        if (!_sm.Sensor.IsBodyTouching || xInput != _sm.Sensor.WallDirection)
        {
            _sm.ChangeState(EPlayerState.Airborne);
            return;
        }
    }

    public void Exit()
    {
        _sm.Input.OnJumpPressed -= HandleWallJump;
    }

    private void HandleWallJump()
    {
        _sm.JumpBufferTimer = 0f;
        float jumpDir = -_sm.Sensor.WallDirection;
        _sm.Motor.SetVelocity(new Vector2(jumpDir * 10f, 10f));
        _sm.ChangeState(EPlayerState.Airborne);
    }
}