using UnityEngine;

public class PlayerAirborneState : IPlayerState
{
    private PlayerStateMachine _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;
    private PlayerDataSO _data;
    private PlayerJump _jump;
    private PlayerSlip _slip;

    public PlayerAirborneState(PlayerStateMachine stateMachine, PlayerDataSO data)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;
        _data = data;
        _jump = new PlayerJump(stateMachine, _motor, data);
        _slip = new PlayerSlip(_sensor, _motor, data);
    }

    public void Enter()
    {
        _jump.Init(_sm.transform.position.x);
        _input.OnJumpReleased += _jump.HandleOnReleased;
        _input.OnDashPressed += HandleDashPressed;
    }

    public void Update()
    {
        HandleHorizontalMovement();
        _jump.CheckLateJump();
        _jump.HandleOnHold();
        _slip.Handle();

        ApplyGravity();
        CheckStateTransitions();
    }

    public void Exit()
    {
        _slip.Reset();
        _input.OnJumpReleased -= _jump.HandleOnReleased;
        _input.OnDashPressed -= HandleDashPressed;
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

    private void ApplyGravity()
    {
        float gravityScale = 1f;
        if (_motor.Velocity.y <= 0f)
        {
            gravityScale = _data.FallGravityMultiplier;
        }
        else if (_jump.IsEarlyReleased)
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
        bool isTouchingWallWithInput =
            !_input.IsWallSlideLocked && _sensor.WallDirection != 0f &&
            _motor.Velocity.y <= 0f && Mathf.Sign(_sensor.WallDirection) == Mathf.Sign(xInput);
        if (!isTouchingWallWithInput)
        {
            return;
        }

        bool isSameWallAsLastJump =
            _sm.LastWallJumpDir != 0f && Mathf.Sign(_sm.LastWallJumpDir) == Mathf.Sign(_sensor.WallDirection);
        if (isSameWallAsLastJump)
        {
            return;
        }

        _sm.ChangeState(EPlayerState.WallSlide);
    }

    private void HandleDashPressed()
    {
        if (_sm.HasUsedHover || 0f < _sm.DashCooldownTimer)
        {
            return;
        }
        _sm.ChangeState(EPlayerState.Hover);
    }
}