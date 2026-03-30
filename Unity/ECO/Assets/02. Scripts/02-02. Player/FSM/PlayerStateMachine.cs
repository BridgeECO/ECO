using System.Collections.Generic;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(PlayerInput), typeof(PlayerSensor), typeof(PlayerMotor))]
public class PlayerStateMachine : MonoBehaviour
{
    [Foldout("Project")]
    [Header("Data")]
    [SerializeField]
    private PlayerDataSO _playerData;

    private IPlayerState _currentState;
    private Dictionary<EPlayerState, IPlayerState> _states;

    public PlayerInput Input { get; private set; }
    public PlayerSensor Sensor { get; private set; }
    public PlayerMotor Motor { get; private set; }
    public Animator Animator { get; private set; }

    public float JumpBufferTimer { get; set; }
    public float CoyoteTimer { get; set; }
    public bool HasUsedHover { get; set; }

    public float InputLockTimer { get; set; }
    public float LastWallJumpDir { get; set; }

    private void Awake()
    {
        Input = GetComponent<PlayerInput>();
        Sensor = GetComponent<PlayerSensor>();
        Motor = GetComponent<PlayerMotor>();
        Animator = GetComponentInChildren<Animator>();

        _states = new Dictionary<EPlayerState, IPlayerState>
        {
            { EPlayerState.Grounded, new PlayerGroundedState(this, _playerData) },
            { EPlayerState.Airborne, new PlayerAirborneState(this, _playerData) },
            { EPlayerState.WallSlide, new PlayerWallSlideState(this, _playerData) },
            { EPlayerState.Hover, new PlayerHoverState(this, _playerData) },
            { EPlayerState.Dash, new PlayerDashState(this, _playerData) }
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
        JumpBufferTimer = Mathf.Max(0f, JumpBufferTimer - Time.deltaTime);
        CoyoteTimer = Mathf.Max(0f, CoyoteTimer - Time.deltaTime);
        InputLockTimer = Mathf.Max(0f, InputLockTimer - Time.deltaTime);
        _currentState?.Update();
    }

    private void OnDisable()
    {
        Input.OnJumpPressed -= HandleJumpPressed;
    }

    private void HandleJumpPressed()
    {
        JumpBufferTimer = _playerData.JumpBufferTime;
    }

    public void ChangeState(EPlayerState newState)
    {
        _currentState?.Exit();
        if (_states.TryGetValue(newState, out IPlayerState state))
        {
            _currentState = state;
            // Debug.Log($"State Transition : {newState}");
            _currentState?.Enter();
        }
        else
        {
            Debug.LogError($"Invalid state transition: {newState}");
        }
    }
}