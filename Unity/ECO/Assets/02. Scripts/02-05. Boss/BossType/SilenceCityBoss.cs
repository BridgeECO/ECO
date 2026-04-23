using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(Rigidbody2D))]
public class SilenceCityBoss : BossBase
{
    [Header("Settings")]
    [SerializeField]
    private BossRoomManager bossRoomManager;

    [Header("Layer Masks")]
    [SerializeField]
    private LayerMask _destructibleLayer;
    [SerializeField]
    private LayerMask _platformLayer;

    private Rigidbody2D _rigidbody;
    private Vector3 _resetPosition;

    private bool _isReset = false;

    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (ReferenceEquals(TargetPlayer, null)) return;

        if (CurrentState == EBossState.Chasing)
        {
            ProcessChaseLogic();
        }
        else
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }

    public void ResetToPosition()
    {
        transform.position = _resetPosition;
        _rigidbody.linearVelocity = Vector2.zero;
    }

    protected override void OnStateChanged(EBossState newState)
    {
        bool isChasing = (newState == EBossState.Chasing);
        if (newState == EBossState.Chasing)
        {
            _resetPosition = transform.position;
        }

        if (AnimationController != null)
        {
            AnimationController.SetChasingState(isChasing);
        }
    }

    private void ProcessChaseLogic()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, TargetPlayer.position);
        float currentSpeed = BossData.BaseSpeed;

        if (distanceToPlayer > BossData.CatchUpDistanceThreshold)
        {
            currentSpeed = BossData.CatchUpSpeed;
        }

        Vector2 direction = (TargetPlayer.position - transform.position).normalized;
        _rigidbody.linearVelocity = new Vector2(direction.x * currentSpeed, _rigidbody.linearVelocity.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (CurrentState != EBossState.Chasing) return;

        if (other.CompareTag(nameof(ETags.Player)))
        {
            if (!_isReset)
            {
                ResetEncounterAsync().Forget();
                Debug.Log($"ResetEncounterAsync ½ÇÇà");

            }
            return;
        }

        int targetLayerMask = (1 << other.gameObject.layer);
        if ((targetLayerMask & _destructibleLayer) != 0 || (targetLayerMask & _platformLayer) != 0) {
            other.gameObject.SetActive(false);
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

        InputHandler.BlockInput();

        try
        {
            var fadeOutUcs = new UniTaskCompletionSource();
            UIManager.Instance.FadeInLoadingPanel(() => fadeOutUcs.TrySetResult());
            await fadeOutUcs.Task;

            if (bossRoomManager != null)
            {
                bossRoomManager.ResetRoom();
            }

            ResetToPosition();

            if (RespawnManager.Instance != null)
            {
                RespawnManager.Instance.Respawn();
            }

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
}
