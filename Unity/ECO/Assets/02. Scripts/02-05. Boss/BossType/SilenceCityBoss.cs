using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SilenceCityBoss : BossBase
{
    [Header("Target Settings")]
    [SerializeField]
    private List<FloorData> _floorDatas;
    [Header("StartBerserkDistance")]
    [SerializeField]
    [UnityEngine.Serialization.FormerlySerializedAs("_starBerserkAnimation")]
    private float _startBerserkDistance;

    private Rigidbody2D _rigidbody;
    private Collider2D _collider;

    private int _currentFloorIndex = 0;
    private CancellationTokenSource _groggyCts;
    private CancellationTokenSource _actionCts;

    private BossPathFollower _pathFollower;
    private ESfxClip? _currentChaseSound = null;
    private bool _isLeft = false;

    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _pathFollower = new BossPathFollower(_rigidbody);

        UpdateCurrentFloorPath();
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
            ProcessChaseLogic();
        }
        else if (CurrentState == EBossState.Idle || CurrentState == EBossState.Groggy)
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }

    protected override void OnStateChanged(EBossState newState)
    {
        AnimationController.SetState(newState);

        if (newState != EBossState.Groggy)
        {
            CancelGroggyTimer();
        }
        if (newState != EBossState.Chasing && newState != EBossState.Berserk)
        {
            SoundManager.Instance.StopLoopSfxOnSource(SfxLoopAudioSource);
            _currentChaseSound = null;
        }

        switch (newState)
        {
            case EBossState.Chasing:
                SetFlip(_isLeft);
                _isLeft = !_isLeft;
                _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                _collider.isTrigger = true;
                break;
            case EBossState.ReadyToJump:
            case EBossState.Jumping:
            case EBossState.Berserk:
                _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                _collider.isTrigger = true;
                break;

            case EBossState.Idle:
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                _collider.isTrigger = true;
                break;
            case EBossState.Groggy:
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                _collider.isTrigger = true;
                StartGroggyTimer().Forget();
                break;
        }
    }
    private void UpdateCurrentFloorPath()
    {
        if (_floorDatas == null || _currentFloorIndex >= _floorDatas.Count)
        {
            _pathFollower.Clear();
            return;
        }

        FloorData currentFloor = _floorDatas[_currentFloorIndex];
        if (currentFloor != null && currentFloor.ChasingLine != null)
        {
            _pathFollower.SetPath(currentFloor.ChasingLine.GetComputedPath());
            return;
        }

        _pathFollower.Clear();
    }

    private void ProcessChaseLogic()
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

        ESfxClip sound = CurrentSpeed == BossData.BaseSpeed ? ESfxClip.SE_TB_Walking : ESfxClip.SE_TB_Rushing;
        if (_currentChaseSound != sound)
        {
            _currentChaseSound = sound;
            SoundManager.Instance.PlayLoopSfxOnSource(sound, SfxLoopAudioSource);
        }

        _pathFollower.Tick(transform.position, CurrentSpeed);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (CurrentState == EBossState.Idle || CurrentState == EBossState.Groggy)
        {
            return;
        }

        if (other.gameObject.CompareTag(nameof(ETags.Player)) && !IsResetting)
        {
            EventManager.Instance.BroadcastEvent(EEventType.PlayerDied);
        }
    }
    protected override void OnBossFrozen()
    {
        _rigidbody.linearVelocity = Vector2.zero;
        CancelActionTasks();
        CancelGroggyTimer();
        SoundManager.Instance.StopLoopSfxOnSource(SfxLoopAudioSource);
        _currentChaseSound = null;
    }
    protected override void OnResetBoss()
    {
        StopChase();
        CancelActionTasks();
        CancelGroggyTimer();

        foreach (var data in _floorDatas)
        {
            if (data.Floor != null)
            {
                data.Floor.ResetFloorTransition();
            }
        }
        _currentFloorIndex = 0;
        UpdateCurrentFloorPath();

        _rigidbody.position = ResetPosition;
        _rigidbody.linearVelocity = Vector2.zero;
        _isLeft = false;
        SetFlip(_isLeft);

    }
    public async UniTask FloorTransition(Transform startPoint, Transform endPoint, float jumpHeight)
    {
        CancelActionTasks();
        CancelGroggyTimer();

        ChangeState(EBossState.ReadyToJump);

        _actionCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        var linkedToken = _actionCts.Token;

        try
        {
            await MoveToPoint(startPoint.position, BossData.CatchUpSpeed, linkedToken);

            ChangeState(EBossState.Jumping);
            SoundManager.Instance.PlaySfxOnSource(ESfxClip.SE_TB_Jump, SfxAudioSource);

            Vector2 startPos = startPoint.position;
            Vector2 endPos = endPoint.position;

            SetFlip(endPos.x - startPos.x < 0.1f);

            float arcLength = BossPhysicsUtility.ApproximateParabolaLength(startPos, endPos, jumpHeight);
            float duration = arcLength / BossData.JumpSpeed;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                Vector2 nextPos = BossPhysicsUtility.GetGeometricParabola(startPos, endPos, jumpHeight, t);
                _rigidbody.MovePosition(nextPos);

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, linkedToken);
            }
            SoundManager.Instance.PlaySfxOnSource(ESfxClip.SE_TB_Land, SfxAudioSource);

            _rigidbody.MovePosition(endPos);
            _rigidbody.linearVelocity = Vector2.zero;

            _currentFloorIndex++;
            UpdateCurrentFloorPath();
            StartChase();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[SilenctCityCoss.cs] FloorTransition");
        }
        finally
        {
            CancelActionTasks();
        }
    }
    private async UniTask MoveToPoint(Vector3 targetPos, float speed, CancellationToken cancellationToken)
    {
        float targetX = targetPos.x;
        float initialDirectionX = Mathf.Sign(targetX - transform.position.x);

        while (this != null)
        {
            float currentDist = targetX - transform.position.x;
            float currentDirectionX = Mathf.Sign(currentDist);

            if (Mathf.Abs(currentDist) <= 0.1f || currentDirectionX != initialDirectionX)
            {
                break;
            }

            _rigidbody.linearVelocity = new Vector2(initialDirectionX * speed, _rigidbody.linearVelocity.y);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken);
        }

        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        _rigidbody.linearVelocity = Vector2.zero;
    }
    private async UniTask StartGroggyTimer()
    {
        SoundManager.Instance.PlaySfxOnSource(ESfxClip.SE_TB_Crash, SfxAudioSource);

        CancelGroggyTimer();

        _groggyCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        var token = _groggyCts.Token;

        try
        {
            if (_floorDatas == null || _currentFloorIndex < 0 || _currentFloorIndex >= _floorDatas.Count || _floorDatas[_currentFloorIndex] == null)
            {
                return;
            }

            float duration = _floorDatas[_currentFloorIndex].GroggyDuration;

            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);

            if (this == null || _floorDatas == null || _currentFloorIndex < 0 || _currentFloorIndex >= _floorDatas.Count)
            {
                return;
            }

            if (_floorDatas[_currentFloorIndex]!=null && _floorDatas[_currentFloorIndex].Floor != null)
            {
                _floorDatas[_currentFloorIndex].Floor.GroggyEnd();
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[SilenctCityCoss.cs] StartGroggyTimer");
        }
        finally
        {
            CancelGroggyTimer();
        }
    }

    private void CancelGroggyTimer()
    {
        if (_groggyCts != null)
        {
            _groggyCts.Cancel();
            _groggyCts.Dispose();
            _groggyCts = null;
        }
    }

    private void CancelActionTasks()
    {
        if (_actionCts is not null)
        {
            _actionCts.Cancel();
            _actionCts.Dispose();
            _actionCts = null;
        }
    }
}
