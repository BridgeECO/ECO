using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(PlayerInput), typeof(PlayerSensor), typeof(PlayerMotor))]
public class PlayerStateMachine : MonoBehaviour, IPlayerFSMContext
{
    public Action<EPlayerState> OnStateChanged;

    [Foldout("Project")]
    [Header("Data")]
    [SerializeField]
    private PlayerDataSO _playerData;

    private IPlayerState _currentState;
    private Dictionary<EPlayerState, IPlayerState> _states;
    private bool _isCutsceneFrozen;

    public PlayerInput Input { get; private set; }
    public PlayerSensor Sensor { get; private set; }
    public PlayerMotor Motor { get; private set; }
    public Animator Animator { get; private set; }
    public Transform Transform => transform;
    public CancellationToken DestroyToken => this.GetCancellationTokenOnDestroy();
    public PlayerSoundHandler SoundHandler { get; private set; }
    public EPlayerState CurrentPlayerState { get; private set; }
    public float JumpBufferTimer { get; set; }
    public float CoyoteTimer { get; set; }
    public bool HasUsedHover { get; set; }
    public float DashCooldownTimer { get; set; }
    public float InputLockTimer { get; set; }
    public float LastWallJumpDir { get; set; }

    public bool IsCutsceneFrozen => _isCutsceneFrozen;

    private void Awake()
    {
        Input = GetComponent<PlayerInput>();
        Sensor = GetComponent<PlayerSensor>();
        Motor = GetComponent<PlayerMotor>();
        Animator = GetComponent<Animator>();
        SoundHandler = new PlayerSoundHandler(Sensor);

        // 플레이어 루트 컴포넌트들의 조립을 SM이 담당한다.
        // (PlayerLife가 SM 내부 소유물을 꺼내 쓰는 대신 여기서 주입)
        PlayerLife playerLife = GetComponent<PlayerLife>();
        if (playerLife != null)
        {
            playerLife.InitSoundHandler(SoundHandler);
        }

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
        Motor.OnTeleported += InitState;
        OnStateChanged += SoundHandler.HandleStateChanged;
    }

    private void Start()
    {
        ChangeState(EPlayerState.Grounded);

        if (SoundManager.HasInstance)
        {
            SoundManager.Instance.SetListener(transform);
        }
    }

    private void Update()
    {
        JumpBufferTimer = Mathf.Max(0f, JumpBufferTimer - Time.deltaTime);
        CoyoteTimer = Mathf.Max(0f, CoyoteTimer - Time.deltaTime);
        InputLockTimer = Mathf.Max(0f, InputLockTimer - Time.deltaTime);
        DashCooldownTimer = Mathf.Max(0f, DashCooldownTimer - Time.deltaTime);

        // 동결 중에는 상태 갱신 자체를 멈춘다. Dash는 입력과 무관하게 매 프레임 속도를 재대입하고
        // Airborne은 중력을 적분하므로, 입력 차단만으로는 제자리에 서지 않는다.
        if (_isCutsceneFrozen)
        {
            return;
        }

        Motor.SetFlip(Input.HorizontalInput);
        _currentState?.Update();

        if (CurrentPlayerState == EPlayerState.Grounded)
        {
            bool isMoving = Input.HorizontalInput != 0f;
            SoundHandler.UpdateWalkSound(Time.deltaTime, isMoving);
        }
    }

    private void OnDisable()
    {
        Input.OnJumpPressed -= HandleJumpPressed;
        Motor.OnTeleported -= InitState;
        OnStateChanged -= SoundHandler.HandleStateChanged;

        // 비활성화는 상태 전이를 거치지 않으므로 루프 SFX를 여기서 직접 끊는다.
        SoundHandler.StopAllLoops();
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
            CurrentPlayerState = newState;
            OnStateChanged?.Invoke(newState);
            _currentState?.Enter();
        }
        else
        {
            Debug.LogError($"Invalid state transition: {newState}");
        }
    }

    /// <summary>
    /// 컷씬이 플레이어를 제자리에 세운다.
    ///
    /// enabled = false를 쓰지 않는다. OnDisable이 Motor.OnTeleported -= InitState를 끊어,
    /// 얼린 채로 죽으면 텔레포트가 FSM을 되살리지 못하고 얼어붙은 상태로 부활한다.
    /// </summary>
    public void SetCutsceneFrozen(bool isFrozen)
    {
        _isCutsceneFrozen = isFrozen;

        if (isFrozen)
        {
            // 정규화를 뒤로 미루면 Airborne.Enter의 PlayerJump가 방금 0으로 만든 속도에
            // 점프 속도를 다시 써 넣는다. 반드시 SetFrozen보다 먼저 끝낸다.
            NormalizeStateForFreeze();
            Animator.SetBool(AnimatorHash.IsRunning, false);
            Animator.ResetTrigger(AnimatorHash.Jump1);
            Animator.ResetTrigger(AnimatorHash.Jump3);
            SoundHandler.StopAllLoops();
        }

        Motor.SetFrozen(isFrozen);
    }

    /// <summary>
    /// 자체 타이머로 도는 상태(Dash·Hover·WallSlide)를 즉시 끊는다.
    /// 버퍼와 코요테를 먼저 비우지 않으면 상태 전이가 재점프를 실행한다.
    /// </summary>
    public void NormalizeStateForFreeze()
    {
        JumpBufferTimer = 0f;
        CoyoteTimer = 0f;
        InputLockTimer = 0f;

        if (CurrentPlayerState == EPlayerState.Grounded || CurrentPlayerState == EPlayerState.Airborne)
        {
            return;
        }

        ChangeState(Sensor.IsOnGround ? EPlayerState.Grounded : EPlayerState.Airborne);
    }

    public void InitState()
    {
        // 컷씬이 해제를 놓친 채 끝나도 여기서 풀린다. 플레이어는 PersistentScene에 상주하고
        // 리전 씬은 언로드되므로, 이 정규 복구 경로가 유일한 안전망이다.
        _isCutsceneFrozen = false;
        Motor.SetFrozen(false);

        InputLockTimer = 0f;
        JumpBufferTimer = 0f;
        CoyoteTimer = 0f;
        DashCooldownTimer = 0f;
        HasUsedHover = false;
        ChangeState(EPlayerState.Grounded);
    }
}
