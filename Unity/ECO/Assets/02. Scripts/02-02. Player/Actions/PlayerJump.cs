using UnityEngine;

public class PlayerJump
{
    private PlayerStateMachine _sm;
    private PlayerMotor _motor;
    private PlayerDataSO _data;

    private bool _isJumpHeld;
    private bool _isEarlyReleased;
    private float _jumpHoldTimer;
    private float _fallOffPosX;

    public bool IsEarlyReleased => _isEarlyReleased;

    public PlayerJump(PlayerStateMachine stateMachine, PlayerMotor motor, PlayerDataSO data)
    {
        _sm = stateMachine;
        _motor = motor;
        _data = data;
    }

    public void Init(float fallOffPosX)
    {
        _fallOffPosX = fallOffPosX;

        if (0f < _sm.JumpBufferTimer && 0f < _sm.CoyoteTimer)
        {
            Execute();
            return;
        }

        _isJumpHeld = false;
        _isEarlyReleased = false;
    }

    private void Execute()
    {
        _sm.JumpBufferTimer = 0f;
        _sm.CoyoteTimer = 0f;
        _isJumpHeld = true;
        _isEarlyReleased = false;
        _jumpHoldTimer = 0f;
        _motor.SetVelocityY(_data.InitialJumpVelocity);
    }

    public void CheckLateJump()
    {
        if (_sm.JumpBufferTimer == 0f || _sm.CoyoteTimer == 0f)
        {
            return;
        }
        if (Mathf.Abs(_sm.transform.position.x - _fallOffPosX) <= _data.CoyoteDistance)
        {
            Execute();
        }
    }

    public void HandleOnHold()
    {
        if (!_isJumpHeld)
        {
            return;
        }
        _jumpHoldTimer += Time.deltaTime;
        if (_jumpHoldTimer < _data.MaxJumpHoldTime)
        {
            return;
        }
        _isJumpHeld = false;
    }

    public void HandleOnReleased()
    {
        if (0f < _sm.InputLockTimer)
        {
            return;
        }

        if (_isJumpHeld)
        {
            _isEarlyReleased = true;
        }
        _isJumpHeld = false;

        if (0f < _motor.Velocity.y)
        {
            _motor.SetVelocityY(_motor.Velocity.y * _data.JumpCutMultiplier);
        }
    }
}
