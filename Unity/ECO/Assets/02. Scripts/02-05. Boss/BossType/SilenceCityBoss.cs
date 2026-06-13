using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[System.Serializable]
public class FloorData
{
    public BossFloorTransition Floor;
    public Transform EndPosition;
    public float GroggyDuration;
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SilenceCityBoss : BossBase
{
    [Header("Target Settings")]
    [SerializeField]
    private List<FloorData> _floorData;

    private Rigidbody2D _rigidbody;
    private Collider2D _collider;

    private bool _isReset = false;
    private bool _isJump = false;
    private int _currentFloorIndex = 0;
    private CancellationTokenSource _groggyCts;
    private CancellationTokenSource _actionCts; //추가?
    private float _currentSpeed;
    private GameObject _player;

    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _player = GameObject.FindWithTag(nameof(ETags.Player));
    }

    private void FixedUpdate()
    {
        if (CurrentState == EBossState.Chasing)
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
        AnimationController.SetChangeState(newState);

        if (newState != EBossState.Groggy)
        {
            CancelGroggyTimer();
        }

        switch (newState)
        {
            case EBossState.Chasing:
                _rigidbody.bodyType = RigidbodyType2D.Dynamic;
                _collider.isTrigger = false;
                break;

            case EBossState.ReadyToJump:
                _rigidbody.bodyType = RigidbodyType2D.Dynamic;
                _collider.isTrigger = false;
                break;

            case EBossState.Jumping:
                _rigidbody.bodyType = RigidbodyType2D.Dynamic;
                _collider.isTrigger = false;
                break;

            case EBossState.Idle:
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                _collider.isTrigger = true;
                break;

            case EBossState.Groggy:
                StartGroggyTimer().Forget();
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                _collider.isTrigger = true;
                break;
            case EBossState.Berserk:
                //광분상태
                break;
        }
    }

    private void ProcessChaseLogic()
    {
        if (_floorData == null || _currentFloorIndex >= _floorData.Count || _floorData[_currentFloorIndex].EndPosition == null)
        {
            return;
        }
        if (_player == null) return;

        Transform currentTarget = _floorData[_currentFloorIndex].EndPosition;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);
        _currentSpeed = (distanceToPlayer >= BossData.CatchUpDistanceThreshold)
            ? BossData.CatchUpSpeed
            : BossData.BaseSpeed;
        //가속도 느낌으로 하려면
        //float smoothedSpeed = Mathf.MoveTowards(Mathf.Abs(_rigidbody.linearVelocity.x), _currentSpeed, Time.fixedDeltaTime * 10f);
        
        Vector2 direction = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;
        _rigidbody.linearVelocity = new Vector2(direction.x * _currentSpeed, _rigidbody.linearVelocity.y);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isJump)
        {
            CheckIsJump(other.gameObject);
        }

        if (CurrentState == EBossState.Idle || CurrentState == EBossState.Groggy) return;

        if (other.gameObject.CompareTag(nameof(ETags.Player)))
        {
            if (!_isReset)
            {
                ResetEncounterAsync().Forget();
            }
            return;
        }else if (other.gameObject.CompareTag(nameof(ETags.Map)))
        {
            return;
        }
        else
        {
            other.gameObject.SetActive(false);
        }
    }

    private void CheckIsJump(GameObject collidedObject)
    {
        if (_isJump && collidedObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            _isJump = false;
            _rigidbody.linearVelocity = Vector2.zero;

            _currentFloorIndex++;
            StartChase();
        }
    }

    private async UniTask ResetEncounterAsync()
    {
        _isReset = true;

        if (UIManager.Instance == null)
        {
            _isReset = false;
            return;
        }

        StopChase();
        CancelActionTasks();
        CancelGroggyTimer();
        _isJump = false;

        InputHandler.BlockInput();

        try
        {
            var fadeOutUcs = new UniTaskCompletionSource();
            UIManager.Instance.FadeInLoadingPanel(() => fadeOutUcs.TrySetResult());
            await fadeOutUcs.Task;

            BossRoomManager.ResetRoom();
            foreach (var data in _floorData)
            {
                if (data.Floor != null)
                {
                    data.Floor.ResetFloorTransition();
                }
            }
            _currentFloorIndex = 0;

            transform.position = ResetPosition;
            _rigidbody.linearVelocity = Vector2.zero;

            RespawnManager.Instance.Respawn();

            await UniTask.Delay(System.TimeSpan.FromSeconds(0.3f));

            var fadeInUcs = new UniTaskCompletionSource();
            UIManager.Instance.FadeOutLoadingPanel(() => fadeInUcs.TrySetResult());
            await fadeInUcs.Task;

            StartChase();
        }
        finally
        {
            _isReset = false;
            InputHandler.UnblockInput();
        }
    }
    public async UniTask FloorTransition(Transform startPoint, Transform endPoint, Vector2 velocity)
    {
        CancelActionTasks();
        CancelGroggyTimer();

        ChangeState(EBossState.ReadyToJump);

        _actionCts = new CancellationTokenSource();
        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(_actionCts.Token, this.GetCancellationTokenOnDestroy()).Token;

        try
        {
            await MoveToPoint(startPoint.position, BossData.CatchUpSpeed, linkedToken);

            ChangeState(EBossState.Jumping);
            _isJump = true;
            _rigidbody.linearVelocity = velocity;

            await UniTask.WaitUntil(() => !_isJump, cancellationToken: linkedToken);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[SilenceCityBoss] FloorTransition 작업이 취소되었습니다.");
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
        CancelGroggyTimer();
        _groggyCts = new CancellationTokenSource();

        try
        {
            float duration = _floorData[_currentFloorIndex].GroggyDuration;

            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: _groggyCts.Token);

            if (_floorData[_currentFloorIndex].Floor != null)
            {
                _floorData[_currentFloorIndex].Floor.GroggyEnd();
            }
        }
        catch (OperationCanceledException)
        {

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
        if (_actionCts != null)
        {
            _actionCts.Cancel();
            _actionCts.Dispose();
            _actionCts = null;
        }
    }
}
