using UnityEngine;

public class PlayerAirborneState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;
    private PlayerDataSO _data;

    private bool _isJumpHeld;
    private bool _isEarlyReleased;
    private float _jumpHoldTimer;
    private float _fallOffPosX;

    public PlayerAirborneState(PlayerStateMachine stateMachine, PlayerDataSO data)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;
        _data = data;
    }

    public void Enter()
    {
        _fallOffPosX = _sm.transform.position.x;

        if (0f < _sm.JumpBufferTimer && 0f < _sm.CoyoteTimer)
        {
            ExecuteJump();
        }
        else
        {
            _isJumpHeld = false;
            _isEarlyReleased = false;
        }

        _input.OnJumpReleased += HandleJumpReleased;
        _input.OnDashPressed += HandleDashPressed;
    }

    public void Update()
    {
        CheckLateJump();
        HandleHorizontalMovement();
        HandleJumpHold();
        HandleSlip();
        ApplyGravity();
        CheckStateTransitions();
    }

    public void Exit()
    {
        _input.OnJumpReleased -= HandleJumpReleased;
        _input.OnDashPressed -= HandleDashPressed;
    }

    private void ExecuteJump()
    {
        _sm.JumpBufferTimer = 0f;
        _sm.CoyoteTimer = 0f;
        _isJumpHeld = true;
        _isEarlyReleased = false;
        _jumpHoldTimer = 0f;
        _motor.SetVelocityY(_data.InitialJumpVelocity);
    }

    private void CheckLateJump()
    {
        if (_sm.JumpBufferTimer == 0f || _sm.CoyoteTimer == 0f)
        {
            return;
        }
        if (Mathf.Abs(_sm.transform.position.x - _fallOffPosX) <= _data.CoyoteDistance)
        {
            ExecuteJump();
        }
    }

    private void HandleHorizontalMovement()
    {
        if (0f < _sm.InputLockTimer)
        {
            return;
        }

        float targetSpeedX = _input.HorizontalInput * _data.GroundMoveSpeed;
        float currentSpeedX = _motor.Velocity.x;
        if (_data.GroundMoveSpeed < Mathf.Abs(currentSpeedX))
        {
            currentSpeedX = Mathf.MoveTowards(currentSpeedX, targetSpeedX, _data.AirDeceleration * Time.deltaTime);
            _motor.SetVelocityX(currentSpeedX);
        }
        else
        {
            _motor.SetVelocityX(targetSpeedX);
        }
    }

    private void HandleJumpHold()
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

    private void HandleSlip()
    {
        if (!_sensor.IsSliding || 0f < _motor.Velocity.y || _sensor.IsGrounded)
        {
            return;
        }
        float velocityX = (_sensor.IsLeftSliding) ? 2f : -2f;
        _motor.SetVelocityX(velocityX);
        _motor.AddVelocity(Vector2.down * _data.SlipDownSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        float gravityScale = 1f;
        if (_motor.Velocity.y < 0f)
        {
            gravityScale = _data.FallGravityMultiplier;
        }
        else if (_isEarlyReleased)
        {
            gravityScale = _data.EarlyReleaseFallMultiplier;
        }
        _motor.AddVelocity(Vector2.down * _data.Gravity * gravityScale * Time.deltaTime);
        if (_motor.Velocity.y < -_data.MaxFallSpeed)
        {
            _motor.SetVelocityY(-_data.MaxFallSpeed);
        }
    }

    private void CheckStateTransitions()
    {
        if (_sensor.IsGrounded && _motor.Velocity.y <= 0f)
        {
            _sm.ChangeState(EPlayerState.Grounded);
            return;
        }

        float xInput = _input.HorizontalInput;
        if (_sensor.IsBodyTouching && _motor.Velocity.y < 0f &&
            Mathf.Sign(_sensor.WallDirection) == Mathf.Sign(xInput))
        {
            if (_sm.LastWallJumpDir != 0f && Mathf.Sign(_sm.LastWallJumpDir) == Mathf.Sign(_sensor.WallDirection))
            {
                return;
            }

            _sm.ChangeState(EPlayerState.WallSlide);
            return;
        }
    }

    private void HandleJumpReleased()
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

    private void HandleDashPressed()
    {
        if (_sm.HasUsedHover)
        {
            return;
        }
        _sm.ChangeState(EPlayerState.Hover);
    }
}