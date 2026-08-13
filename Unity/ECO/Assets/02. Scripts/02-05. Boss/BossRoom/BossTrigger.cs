using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using VInspector;

public class BossTrigger : MonoBehaviour, IResettable
{
    public enum ETriggerTarget { Player, Boss }

    [Foldout("Trigger Settings")]
    [SerializeField]
    [Tooltip("트리거를 발동시킬 대상을 설정하세요.")]
    private ETriggerTarget _targetType = ETriggerTarget.Player;

    [SerializeField]
    [Tooltip("이 트리거가 제어할 보스입니다. 새 보스전에서는 직접 할당하세요.")]
    private BossBase _targetBoss;

    [HideInInspector]
    [SerializeField]
    private EBoss _targetBossType;
    [SerializeField]
    [Tooltip("이 트리거에 반응할 보스의 행동 상태를 설정하세요.")]
    private EBossState _triggerAction = EBossState.Chasing;
    [SerializeField]
    [Tooltip("보스전이 리셋되었을 때 이 트리거를 다시 작동하게 할지 설정하세요.")]
    private bool _isResettable = true;

    [Foldout("Cinematic")]
    [SerializeField]
    [Tooltip("재생할 보스 시네마틱 컴포넌트.")]
    private BossCinematicBase _cinematicSequence;

    private bool _hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered)
        {
            return;
        }

        if (_targetType == ETriggerTarget.Player && other.CompareTag(nameof(ETags.Player)))
        {
            ExecuteAction();
        }
        else if (_targetType == ETriggerTarget.Boss && other.CompareTag(nameof(ETags.Boss)))
        {
            ExecuteAction();
        }
    }

    private void ExecuteAction()
    {
        if (_targetBoss == null)
        {
            _targetBoss = BossManager.Instance.GetBoss(_targetBossType);
            if (_targetBoss == null)
            {
                return;
            }
        }

        _hasTriggered = true;

        CancellationToken triggerToken = this.GetCancellationTokenOnDestroy();
        CancellationToken bossToken = _targetBoss.GetCancellationTokenOnDestroy();
        StartCinematic(_targetBoss, triggerToken, bossToken).Forget();
    }

    private async UniTask StartCinematic(
        BossBase boss,
        CancellationToken triggerToken,
        CancellationToken bossToken)
    {
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            triggerToken,
            bossToken);
        CancellationToken cancellationToken = linkedCts.Token;

        try
        {
            if (_cinematicSequence != null)
            {
                await _cinematicSequence.PlayCinematicAsync(boss, cancellationToken);
            }
            else
            {
                ExecuteStateAction(boss);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ExecuteStateAction(BossBase boss)
    {
        if (_triggerAction == EBossState.Chasing)
        {
            boss.StartChase();
        }
        else if (_triggerAction == EBossState.Groggy)
        {
            boss.StartGroggy();
        }
        else if (_triggerAction == EBossState.Idle)
        {
            boss.StopChase();
        }
    }

    public void ResetState()
    {
        if (_isResettable)
        {
            _hasTriggered = false;
        }
    }
}
