using UnityEngine;

public class PlayerAirborneState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;

    private float _maxFallSpeed;
    private float _gravity;
    private float _jumpSpeed;
    private float _airMoveSpeed;
    private float _maxJumpHoldTime;
    private float _slipDownSpeed;

    private bool _isJumping;
    private float _jumpHoldTimer;

    public PlayerAirborneState(PlayerStateMachine stateMachine, PlayerDataSO data)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;

        _maxFallSpeed = data.MaxFallSpeed;
        _gravity = data.Gravity;
        _jumpSpeed = data.JumpSpeed;
        _airMoveSpeed = data.AirMoveSpeed;
        _maxJumpHoldTime = data.MaxJumpHoldTime;
        _slipDownSpeed = data.SlipDownSpeed;
    }

    public void Enter()
    {
        if (_sm.JumpBufferTimer > 0f && _sm.CoyoteTimer > 0f)
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
        ApplyGravity();
        HandleJumpHold();
        HandleSlip();
        CheckStateTransitions();
    }

    public void Exit()
    {
        _input.OnJumpReleased -= HandleJumpReleased;
        _input.OnDashPressed -= HandleDashPressed;
    }

    private void CheckLateJump()
    {
        if (_sm.JumpBufferTimer > 0f && _sm.CoyoteTimer > 0f)
        {
            ExecuteJump();
        }
    }

    private void HandleHorizontalMovement()
    {
        if (0f < _sm.InputLockTimer)
        {
            Debug.Log(_sm.InputLockTimer);
            return;
        }

        float xInput = _input.HorizontalInput;
        _motor.SetVelocityX(xInput * _airMoveSpeed);
    }

    private void HandleJumpHold()
    {
        if (_isJumping)
        {
            _jumpHoldTimer += Time.deltaTime;
            if (_maxJumpHoldTime <= _jumpHoldTimer)
            {
                _isJumping = false;
            }
        }
    }

    private void HandleSlip()
    {
        if (_sensor.IsSliding && !_sensor.IsGrounded)
        {
            _motor.AddVelocity(Vector2.down * _slipDownSpeed * Time.deltaTime);
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
        if (_sensor.IsBodyTouching && xInput != 0f &&
            Mathf.Sign(_sensor.WallDirection) == Mathf.Sign(xInput) &&
            _motor.Velocity.y < 0f)
        {
            if (_sm.LastWallJumpDir != 0f && Mathf.Sign(_sm.LastWallJumpDir) == Mathf.Sign(_sensor.WallDirection))
            {
                return;
            }
            _sm.ChangeState(EPlayerState.WallSlide);
            return;
        }
    }

    private void ApplyGravity()
    {
        if (_isJumping)
        {
            return;
        }
        _motor.AddVelocity(Vector2.down * _gravity * Time.deltaTime);
        if (_motor.Velocity.y < -_maxFallSpeed)
        {
            _motor.SetVelocityY(-_maxFallSpeed);
        }
    }

    private void ExecuteJump()
    {
        _sm.JumpBufferTimer = 0f;
        _sm.CoyoteTimer = 0f;
        _isJumping = true;
        _jumpHoldTimer = 0f;
        _motor.SetVelocityY(_jumpSpeed);
    }

    private void HandleJumpReleased()
    {
        _isJumping = false;
        if (0f < _motor.Velocity.y)
        {
            _motor.SetVelocityY(0f);
        }
    }

    private void HandleDashPressed()
    {
        if (!_sm.HasUsedHover)
        {
            _sm.ChangeState(EPlayerState.Hover);
        }
    }
}