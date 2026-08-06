using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using VInspector;

public abstract class BossBase : MonoBehaviour
{
    public Action OnBossDefeated;

    [Foldout("Identity")]
    [SerializeField]
    private BossDataSO _bossData;
    [SerializeField]
    private EBoss _bossType;

    [Foldout("Hierarchy")]
    [SerializeField]
    private BossAnimationController _animationController;

    [Header("Sound")]
    [SerializeField]
    private AudioSource _sfxAudioSource;
    [SerializeField]
    private AudioSource _sfxLoopAudioSource;

    [SerializeField] 
    private EBossState _currentState;
    private Vector3 _resetPosition;

    private bool _isResetting;
    private bool _isFrozen;
    private SpriteRenderer _spriteRenderer;
    private BossChaseSpeedResolver _speedResolver;

    protected BossDataSO BossData => _bossData;
    protected BossAnimationController AnimationController => _animationController;
    protected EBossState CurrentState { get => _currentState; private set => _currentState = value; }
    protected Vector3 ResetPosition { get => _resetPosition; private set => _resetPosition = value; }
    protected float CurrentSpeed { get; private set; }
    protected AudioSource SfxAudioSource => _sfxAudioSource;
    protected AudioSource SfxLoopAudioSource => _sfxLoopAudioSource;
    protected bool IsResetting => _isResetting;
    protected bool IsFrozen => _isFrozen;
    protected virtual void Awake()
    {
        if (_animationController == null)
        {
            _animationController = GetComponentInChildren<BossAnimationController>();
        }

        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (BossManager.Instance != null)
        {
            BossManager.Instance.RegisterBoss(_bossType, this);
        }

        _speedResolver = new BossChaseSpeedResolver(BossData);
    }
    
    protected virtual void Start()
    {
        InitBoss();
        ResetPosition = transform.position;
        CurrentSpeed = BossData.BaseSpeed;

    }

    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.AddEventListener(EEventType.PlayerDied, OnPlayerDied);
            EventManager.Instance.AddEventListener(EEventType.RespawnReset, OnRespawnReset);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (IsFrozen)
        {
            return;
        }

        if (CurrentState == EBossState.Chasing || CurrentState == EBossState.Berserk)
        {
            CurrentSpeed = _speedResolver.Resolve(
                transform.position,
                CurrentState,
                CurrentSpeed);
        }
    }

    private void OnDisable()
    {
        if (MonoBehaviourSingleton<EventManager>.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.PlayerDied, OnPlayerDied);
            EventManager.Instance.RemoveEventListener(EEventType.RespawnReset, OnRespawnReset);
        }
    }

    protected virtual void OnDestroy()
    {
        if (BossManager.HasInstance)
        {
            BossManager.Instance.UnregisterBoss(_bossType);
        }
    }

    public virtual void InitBoss()
    {
        ChangeState(EBossState.Idle);
    }

    public virtual void StartChase() => ChangeState(EBossState.Chasing);
    public virtual void StopChase() => ChangeState(EBossState.Idle);
    public virtual void StartGroggy() => ChangeState(EBossState.Groggy);

    protected void ChangeState(EBossState newState)
    {
        if (CurrentState == newState)
        {
            return;
        }
        CurrentState = newState;
        OnStateChanged(newState);
        if (newState == EBossState.Chasing)
        {
            SoundManager.Instance.PlayBgm(EBgmType.SyrBossRoom);
        }
    }

    protected abstract void OnStateChanged(EBossState newState);

    private void OnPlayerDied()
    {
        if (_isFrozen)
        {
            return;
        }

        _isFrozen = true;
        AnimationController.Pause();
        OnBossFrozen();
    }

    private void OnRespawnReset()
    {
        if (_isResetting)
        {
            return;
        }

        ResetBoss();
        SoundManager.Instance.StopBgm();
    }

    protected void SetFlip(bool isLeft)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.flipX = isLeft;
        }
    }

    private void ResetBoss()
    {
        _isResetting = true;

        try
        {
            OnResetBoss();
        }
        finally
        {
            AnimationController.Resume();
            _isFrozen = false;
            _isResetting = false;
        }
    }

    protected abstract void OnBossFrozen();
    protected abstract void OnResetBoss();

    public async UniTask WaitForStateAsync(EBossState targetState, CancellationToken cancellationToken)
    {
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            this.GetCancellationTokenOnDestroy());

        await UniTask.WaitUntil(
            () => CurrentState == targetState,
            cancellationToken: linkedCts.Token);
    }

    public virtual void PlayShoutSfx()
    {
        SoundManager.Instance.PlaySfxOnSource(BossData.ShoutSfx, SfxAudioSource);
    }
}
