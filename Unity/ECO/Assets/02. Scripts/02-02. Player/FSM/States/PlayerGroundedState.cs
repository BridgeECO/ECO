using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerGroundedState : IPlayerState
{
    private IPlayerFsmContext _sm;
    private PlayerInput _input;
    private PlayerSensor _sensor;
    private PlayerMotor _motor;
    private PlayerDataSO _data;
    private PlayerUnderJump _underJump;

    public PlayerGroundedState(IPlayerFsmContext stateMachine, PlayerDataSO data)
    {
        _sm = stateMachine;
        _input = stateMachine.Input;
        _sensor = stateMachine.Sensor;
        _motor = stateMachine.Motor;
        _data = data;
        _underJump = new PlayerUnderJump();
    }

    public void Enter()
    {
        _sm.HasUsedHover = false;
        _sm.CoyoteTimer = _data.CoyoteTime;
        _sm.LastWallJumpDir = 0f;
        _motor.SetVelocityY(0f);

        _input.OnDashPressed += HandleDashPressed;
    }

    public void Update()
    {
        _sm.CoyoteTimer = _data.CoyoteTime;
        Run();

        if (0f < _sm.JumpBufferTimer)
        {
            if (_input.VerticalInput < 0f)
            {
                if (_sensor.IsOnPlatform)
                {
                    _sm.JumpBufferTimer = 0f;
                    _underJump.ExecuteAsync(_sensor.CurrentPlatformEffector, _sm.DestroyToken).Forget();
                    _sm.ChangeState(EPlayerState.Airborne);
                    return;
                }
            }

            _sm.ChangeState(EPlayerState.Airborne);
            return;
        }

        if (!_sensor.IsOnGround)
        {
            _sm.ChangeState(EPlayerState.Airborne);
            return;
        }
    }

    public void Exit()
    {
        _input.OnDashPressed -= HandleDashPressed;
    }

    private void HandleDashPressed()
    {
        if (_sm.HasUsedHover)
        {
            return;
        }
        if (0f < _sm.DashCooldownTimer)
        {
            return;
        }
        _sm.ChangeState(EPlayerState.Hover);
    }

    private void Run()
    {
        float xInput = _input.HorizontalInput;
        _motor.SetVelocityX(xInput * _data.GroundMoveSpeed);
        _sm.Animator.SetBool(AnimatorHash.IsRunning, xInput != 0f);
    }
}