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
    [SerializeField]
    private BossRoomManager _bossRoomManager;

    [SerializeField] 
    private EBossState _currentState;
    private Vector3 _resetPosition;

    public BossDataSO BossData { get => _bossData; protected set => _bossData = value; }
    public EBoss BossType => _bossType;
    protected BossAnimationController AnimationController => _animationController;
    protected BossRoomManager BossRoomManager => _bossRoomManager;
    protected EBossState CurrentState { get => _currentState; private set => _currentState = value; }
    protected Vector3 ResetPosition { get => _resetPosition; private set => _resetPosition = value; }

    protected virtual void Awake()
    {
        if (_animationController == null)
        {
            _animationController = GetComponentInChildren<BossAnimationController>();
        }

        if (BossManager.Instance != null)
        {
            BossManager.Instance.RegisterBoss(_bossType, this);
        }
    }
    private void Start()
    {
        InitBoss();
        ResetPosition = transform.position;
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
    }
    protected abstract void OnStateChanged(EBossState newState);

    public async UniTask WaitForStateAsync(EBossState targetState)
    {
        await UniTask.WaitUntil(() => this.CurrentState == targetState);
    }
}
