using System;
using UnityEngine;
using VInspector;

public abstract class BossBase : MonoBehaviour
{
    public Action OnBossDefeated;

    [Foldout("Project")]
    [SerializeField]
    private BossDataSO _bossData;

    [Foldout("Hierarchy")]
    [SerializeField]
    private BossAnimationController _animationController;
    [SerializeField]
    protected BossRoomManager _bossRoomManager;

    protected Transform TargetPlayer;

    public BossDataSO BossData { get => _bossData; protected set => _bossData = value; }
    protected BossAnimationController AnimationController => _animationController;
    protected EBossState CurrentState { get; private set; } = EBossState.Idle;

    protected virtual void Awake()
    {
        if (_animationController == null)
        {
            _animationController = GetComponentInChildren<BossAnimationController>();
        }

        if (_bossRoomManager == null)
        {
            _bossRoomManager = UnityEngine.Object.FindFirstObjectByType<BossRoomManager>();
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


}
