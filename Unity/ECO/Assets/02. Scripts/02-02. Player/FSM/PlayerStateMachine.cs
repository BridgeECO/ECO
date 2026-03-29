using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerInput), typeof(PlayerSensor), typeof(PlayerMotor))]
public class PlayerStateMachine : MonoBehaviour
{
    public PlayerInput Input { get; private set; }
    public PlayerSensor Sensor { get; private set; }
    public PlayerMotor Motor { get; private set; }
    public Animator Animator { get; private set; }

    public float JumpBufferTimer { get; set; }
    public float CoyoteTimer { get; set; }
    public bool HasUsedHover { get; set; }

    private IPlayerState _currentState;
    private Dictionary<EPlayerState, IPlayerState> _states;

    private void Awake()
    {
        Input = GetComponent<PlayerInput>();
        Sensor = GetComponent<PlayerSensor>();
        Motor = GetComponent<PlayerMotor>();
        Animator = GetComponentInChildren<Animator>();

        _states = new Dictionary<EPlayerState, IPlayerState>
        {
            { EPlayerState.Grounded, new PlayerGroundedState(this) },
            { EPlayerState.Airborne, new PlayerAirborneState(this) },
            { EPlayerState.WallSlide, new PlayerWallSlideState(this) },
            { EPlayerState.Hover, new PlayerHoverState(this) },
            { EPlayerState.Dash, new PlayerDashState(this) }
        };
    }

    private void OnEnable()
    {
        Input.OnJumpPressed += HandleJumpPressed;
    }

    private void Start()
    {
        ChangeState(EPlayerState.Grounded);
    }

    private void Update()
    {
        if (JumpBufferTimer > 0) JumpBufferTimer -= Time.deltaTime;
        if (CoyoteTimer > 0) CoyoteTimer -= Time.deltaTime;

        _currentState?.Update();
    }

    private void OnDisable()
    {
        Input.OnJumpPressed -= HandleJumpPressed;
    }

    private void HandleJumpPressed()
    {
        JumpBufferTimer = 0.2f;
    }

    public void ChangeState(EPlayerState newState)
    {
        _currentState?.Exit();
        _currentState = _states[newState];
        _currentState?.Enter();
    }
}