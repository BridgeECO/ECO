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
    [SerializeField]
    private BossRoomManager _bossRoomManager;

    private EBossState _currentState;
    protected Transform TargetPlayer;

    public BossDataSO BossData { get => _bossData; protected set => _bossData = value; }
    public EBoss BossType => _bossType;
    protected BossAnimationController AnimationController => _animationController;
    protected BossRoomManager BossRoomManager => _bossRoomManager;
    protected EBossState CurrentState { get => _currentState; private set => _currentState = value; }

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

        GameObject player = GameObject.FindWithTag(nameof(ETags.Player));
        if (player != null)
        {
            TargetPlayer = player.transform;
        }
    }

    public virtual void InitBoss()
    {
        ChangeState(EBossState.Idle);
    }

    public virtual void StartChase() => ChangeState(EBossState.Chasing);
    public virtual void StopChase() => ChangeState(EBossState.Idle);

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

    protected virtual void OnDestroy()
    {
        if (BossManager.Instance != null)
        {
            BossManager.Instance.UnregisterBoss(_bossType);
        }
    }
}
