using UnityEngine;

public class PlayerAirborneState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;
    private float maxFallSpeed = 5.33f;
    private float gravity = 20f;
    private float jumpSpeed = 10f;

    private bool isJumping;
    private float jumpHoldTimer;

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
            isJumping = false;
        }

        _input.OnJumpReleased += HandleJumpReleased;
        _input.OnDashPressed += HandleDashPressed;
    }

    public void Update()
    {
        float xInput = _input.HorizontalInput;
        _motor.SetVelocityX(xInput * 2f);
        ApplyGravity();

        if (isJumping)
        {
            jumpHoldTimer += Time.deltaTime;
            if (1.5f <= jumpHoldTimer)
            {
                isJumping = false;
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
        if (!isJumping)
        {
            _sm.Motor.AddVelocity(Vector2.down * gravity * Time.deltaTime);
            if (_sm.Motor.Velocity.y < -maxFallSpeed)
            {
                _sm.Motor.SetVelocityY(-maxFallSpeed);
            }
        }
    }

    private void ExecuteJump()
    {
        _sm.JumpBufferTimer = 0f;
        _sm.CoyoteTimer = 0f;
        isJumping = true;
        jumpHoldTimer = 0f;
        _motor.SetVelocityY(jumpSpeed);
    }

    private void HandleJumpReleased()
    {
        isJumping = false;
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