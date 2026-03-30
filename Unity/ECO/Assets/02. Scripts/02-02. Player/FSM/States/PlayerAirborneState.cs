using UnityEngine;

public class PlayerAirborneState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;
    private float _maxFallSpeed = 5.33f;
    private float _gravity = 20f;
    private float _jumpSpeed = 5.33f;
    private bool _isJumping;
    private float _jumpHoldTimer;

    public PlayerAirborneState(PlayerStateMachine stateMachine)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;
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
        float xInput = _input.HorizontalInput;
        _motor.SetVelocityX(xInput * 2f);
        ApplyGravity();

        if (_isJumping)
        {
            _jumpHoldTimer += Time.deltaTime;
            if (1.5f <= _jumpHoldTimer)
            {
                _isJumping = false;
            }
        }

        if (_sensor.IsSliding && !_sensor.IsGrounded)
        {
            _motor.AddVelocity(Vector2.down * 15f * Time.deltaTime);
        }

        if (_sensor.IsGrounded && _motor.Velocity.y <= 0f)
        {
            _sm.ChangeState(EPlayerState.Grounded);
            return;
        }

        if (_sensor.IsBodyTouching && xInput != 0f &&
            Mathf.Sign(_sensor.WallDirection) == Mathf.Sign(xInput) &&
            _motor.Velocity.y < 0f)
        {
            _sm.ChangeState(EPlayerState.WallSlide);
            return;
        }
    }

    public void Exit()
    {
        _input.OnJumpReleased -= HandleJumpReleased;
        _input.OnDashPressed -= HandleDashPressed;
    }

    private void ApplyGravity()
    {
        if (!_isJumping)
        {
            _sm.Motor.AddVelocity(Vector2.down * _gravity * Time.deltaTime);
            if (_sm.Motor.Velocity.y < -_maxFallSpeed)
            {
                _sm.Motor.SetVelocityY(-_maxFallSpeed);
            }
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
        if (0f < _sm.Motor.Velocity.y)
        {
            _sm.Motor.SetVelocityY(0f);
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