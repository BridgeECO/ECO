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
                Debug.Log($"ResetEncounterAsync 실행");

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
        StopChase();
        // SceneTransitionManager에서 쓰신 것처럼 플레이어 입력을 막습니다.
        // (InputHandler 스크립트가 전역으로 접근 가능하다고 가정)
        InputHandler.BlockInput();

        try
        {
            // [1단계] 페이드 인 (화면이 어두워짐)
            var fadeOutUcs = new UniTaskCompletionSource();
            UIManager.Instance.FadeInLoadingPanel(() => fadeOutUcs.TrySetResult());
            await fadeOutUcs.Task;

            // [2단계] 화면이 완전히 가려진 상태에서 모든 요소 리셋
            if (bossRoomManager != null)
            {
                bossRoomManager.ResetRoom();
            }
            ResetToPosition(); // 보스를 처음 추격 시작 위치로 이동

            if (RespawnManager.Instance != null)
            {
                RespawnManager.Instance.Respawn(); // 플레이어 부활 지점으로 이동
            }

            // (선택 사항) 화면이 닫힌 상태로 아주 약간의 대기 시간을 주면 연출이 더 자연스럽습니다.
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.3f));

            // [3단계] 페이드 아웃 (화면이 다시 밝아짐)
            var fadeInUcs = new UniTaskCompletionSource();
            UIManager.Instance.FadeOutLoadingPanel(() => fadeInUcs.TrySetResult());
            await fadeInUcs.Task;

            // [4단계] 페이드 아웃이 완료되면 보스가 다시 추격을 시작
            StartChase();
        }
        finally
        {
            // 모든 연출이 끝난 후 플래그 해제 및 조작 권한 복구
            _isReset = false;
            InputHandler.UnblockInput();
        }
    }
}
