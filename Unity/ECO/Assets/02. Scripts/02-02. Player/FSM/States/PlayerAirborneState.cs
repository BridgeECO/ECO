using UnityEngine;

public class PlayerAirborneState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;
    private PlayerDataSO _data;

    private bool _isJumping;
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
            _isJumping = false;
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
        _isJumping = true;
        _jumpHoldTimer = 0f;
        _motor.SetVelocityY(_data.JumpHeight / _data.MaxJumpHoldTime);
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

        float targetSpeedX = _input.HorizontalInput * _data.AirMoveSpeed;
        float currentSpeedX = _motor.Velocity.x;
        if (_data.AirMoveSpeed < Mathf.Abs(currentSpeedX))
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
        if (!_isJumping)
        {
            return;
        }
        _jumpHoldTimer += Time.deltaTime;

        if (_jumpHoldTimer < _data.MaxJumpHoldTime)
        {
            return;
        }
        _isJumping = false;
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
        if (_isJumping)
        {
            return;
        }
        float gravityScale = (_motor.Velocity.y < 0f) ? _data.FallGravityMultiplier : 1f;
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
        _isJumping = false;
        if (0f < _sm.InputLockTimer || 0f == _motor.Velocity.y)
        {
            return;
        }
        _motor.SetVelocityY(0f);
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