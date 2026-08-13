using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BossChasingCharacter : BossBase
{
    [Header("Battle Terrain")]
    [SerializeField]
    private List<FloorData> _floorDatas = new List<FloorData>();

    [Header("Berserk")]
    [SerializeField]
    [UnityEngine.Serialization.FormerlySerializedAs("_starBerserkAnimation")]
    private float _startBerserkDistance = 30f;
    private Rigidbody2D _rigidbody;
    private Collider2D _collider;
    private BossPathFollower _pathFollower;
    private BossTransitionMover _transitionMover;
    private BossChaseAudioController _chaseAudioController;
    private BossGroggyTimer _groggyTimer;
    private BossBattleTerrainController _terrainController;
    private bool _isLeft;
    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _pathFollower = new BossPathFollower(_rigidbody);
        _transitionMover = new BossTransitionMover(_rigidbody, transform);
        _chaseAudioController = new BossChaseAudioController(
            BossData,
            SfxAudioSource,
            SfxLoopAudioSource);
        _groggyTimer = new BossGroggyTimer();
        _terrainController = new BossBattleTerrainController(_floorDatas, _pathFollower);
        _terrainController.SetCurrentPath();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (IsFrozen)
        {
            return;
        }
        if (CurrentState == EBossState.Chasing || CurrentState == EBossState.Berserk)
        {
            ProcessChase();
            return;
        }
        if (CurrentState == EBossState.Idle || CurrentState == EBossState.Groggy)
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (CurrentState == EBossState.Idle || CurrentState == EBossState.Groggy)
        {
            return;
        }
        if (other.CompareTag(nameof(ETags.Player)) && !IsResetting)
        {
            EventManager.Instance.BroadcastEvent(EEventType.PlayerDied);
        }
    }

    protected override void OnDestroy()
    {
        CancelTasks();
        base.OnDestroy();
    }
    protected override void OnStateChanged(EBossState newState)
    {
        AnimationController.SetState(newState);
        SetPhysicsState();

        if (newState != EBossState.Groggy)
        {
            _groggyTimer.Cancel();
        }
        if (newState == EBossState.Chasing)
        {
            SetFlip(_isLeft);
            _isLeft = !_isLeft;
        }
        else if (newState == EBossState.Groggy)
        {
            StartGroggyTimerAsync().Forget();
        }
        if (newState != EBossState.Chasing && newState != EBossState.Berserk)
        {
            _chaseAudioController.StopChase();
        }
    }

    protected override void OnBossFrozen()
    {
        _rigidbody.linearVelocity = Vector2.zero;
        CancelTasks();
        _chaseAudioController.StopChase();
    }
    protected override void OnResetBoss()
    {
        StopChase();
        CancelTasks();
        _terrainController.Reset();
        _rigidbody.position = ResetPosition;
        _rigidbody.linearVelocity = Vector2.zero;
        _isLeft = false;
        SetFlip(false);
    }

    public async UniTask TransitionFloorAsync(
        Transform startPoint,
        Transform endPoint,
        float jumpHeight,
        CancellationToken cancellationToken)
    {
        _groggyTimer.Cancel();
        ChangeState(EBossState.ReadyToJump);

        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            this.GetCancellationTokenOnDestroy());
        try
        {
            await _transitionMover.MoveToPointAsync(startPoint.position, BossData.CatchUpSpeed, linkedCts.Token);
            ChangeState(EBossState.Jumping);
            SetFlip(endPoint.position.x < startPoint.position.x);
            SoundManager.Instance.PlaySfxOnSource(ESfxClip.SE_TB_Jump, SfxAudioSource);
            await _transitionMover.JumpAsync(startPoint.position, endPoint.position, jumpHeight, BossData.JumpSpeed, linkedCts.Token);
            SoundManager.Instance.PlaySfxOnSource(ESfxClip.SE_TB_Land, SfxAudioSource);
            _terrainController.Advance();
            StartChase();
        }
        catch (OperationCanceledException)
        {
            if (this != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                StopChase();
            }
        }
        finally
        {
            _transitionMover.Cancel();
        }
    }

    private void ProcessChase()
    {
        if (!_pathFollower.HasPath)
        {
            return;
        }
        if (CurrentState == EBossState.Chasing &&
            Vector3.Distance(transform.position, _pathFollower.EndPoint) <= _startBerserkDistance)
        {
            ChangeState(EBossState.Berserk);
        }

        _chaseAudioController.RefreshChase(CurrentSpeed);
        _pathFollower.Tick(transform.position, CurrentSpeed);
    }

    private void SetPhysicsState()
    {
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _collider.isTrigger = true;
    }

    private async UniTask StartGroggyTimerAsync()
    {
        FloorData floorData = _terrainController.CurrentFloorData;
        if (floorData == null)
        {
            return;
        }
        _chaseAudioController.PlayCrash();
        bool hasCompleted = await _groggyTimer.WaitAsync(
            floorData.GroggyDuration,
            this.GetCancellationTokenOnDestroy());
        if (hasCompleted && floorData.Floor != null)
        {
            floorData.Floor.StartTransition();
        }
    }

    private void CancelTasks()
    {
        _groggyTimer.Cancel();
        _transitionMover.Cancel();
    }
}
