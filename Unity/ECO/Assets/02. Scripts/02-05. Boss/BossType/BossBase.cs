using Cysharp.Threading.Tasks;
using System;
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

    private bool _isReset = false;
    private Transform _playerTransform;
    private SpriteRenderer _spriteRenderer;

    protected BossDataSO BossData => _bossData;
    protected BossAnimationController AnimationController => _animationController;
    protected EBossState CurrentState { get => _currentState; private set => _currentState = value; }
    protected Vector3 ResetPosition { get => _resetPosition; private set => _resetPosition = value; }
    protected float CurrentSpeed { get; private set; }
    protected AudioSource SfxAudioSource => _sfxAudioSource;
    protected AudioSource SfxLoopAudioSource => _sfxLoopAudioSource;
    protected bool IsReset { get; set; }
    protected Transform PlayerTransform => _playerTransform;
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
    }
    
    protected virtual void Start()
    {
        InitBoss();
        ResetPosition = transform.position;
        CurrentSpeed = BossData.BaseSpeed;

        GameObject playerObj = GameObject.FindWithTag(nameof(ETags.Player));
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
        }
    }

    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.AddEventListener(EEventType.PlayerDied, OnPlayerDied);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (CurrentState == EBossState.Chasing || CurrentState == EBossState.Berserk)
        {
            UpdateCurrentSpeed();
        }
    }

    private void OnDisable()
    {
        if (MonoBehaviourSingleton<EventManager>.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.PlayerDied, OnPlayerDied);
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

    private void UpdateCurrentSpeed()
    {
        if(CurrentState == EBossState.Berserk)
        {
            CurrentSpeed = BossData.CatchUpSpeed;
            return;
        }

        if (_playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag(nameof(ETags.Player));
            if (playerObj != null)
            {
                _playerTransform = playerObj.transform;
            }
            else
            {
                CurrentSpeed = BossData.BaseSpeed;
                return;
            }
        }

        float distance = Vector2.Distance(transform.position, _playerTransform.position);

        if (distance >= BossData.CatchUpStartDistance)
        {
            CurrentSpeed = BossData.CatchUpSpeed;
        }
        else if (distance <= BossData.CatchUpEndDistance)
        {
            CurrentSpeed = BossData.BaseSpeed;
        }
    }

    private void OnPlayerDied()
    {
        if (!_isReset || CurrentState != EBossState.Idle)
        {
            ResetBoss();
            SoundManager.Instance.StopBgm();
        }
    }

    protected void SetFlip(bool isLeft)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.flipX = isLeft;
        }
    }

    protected virtual void ResetBoss()
    {
        _isReset = true;

        if (UIManager.Instance == null)
        {
            _isReset = false;
            return;
        }
    }

    public async UniTask WaitForStateAsync(EBossState targetState)
    {
        await UniTask.WaitUntil(() => this.CurrentState == targetState);
    }

    public virtual void PlayShoutSfx()
    {
        SoundManager.Instance.PlaySfxOnSource(BossData.ShoutSfx, SfxAudioSource);
    }
}
