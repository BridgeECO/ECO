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
        Debug.Log($"상태 전환 : {newState}");
        _currentState?.Enter();
    }
}