using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Threading;
using VInspector;

public class EndChasingCinematic : BossCinematicBase
{
    [Foldout("Cinematic Settings")]
    [SerializeField]
    [Tooltip("보스에게 카메라가 이동하는 시간")]
    private float _panToBossDuration;
    [SerializeField]
    [Tooltip("카메라를 흔드는 시간")]
    private float _shakeDuration;
    [SerializeField]
    [Tooltip("포효 시 카메라 흔들림 강도")]
    private float _shakeStrength;
    [SerializeField]
    [Tooltip("플레이어에게 다시 돌아오는 시간")]
    private float _returnDuration;

    public override async UniTask PlayCinematicAsync(BossBase boss)
    {
        if (boss == null || _camController == null || _camEffect == null)
        {
            return;
        }

        CancellationToken cinematicToken = this.GetCancellationTokenOnDestroy();
        CancellationToken bossToken = boss.GetCancellationTokenOnDestroy();
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cinematicToken,
            bossToken);
        CancellationToken cancellationToken = linkedCts.Token;

        InputHandler.BlockInput();
        _camController.IsFollowingPlayer = false;

        try
        {
            UniTask waitForGroggy = boss.WaitForStateAsync(EBossState.Groggy, cancellationToken);
            while (boss != null && waitForGroggy.Status == UniTaskStatus.Pending)
            {
                Vector3 targetPos = _camController.GetClampedPosition(boss.transform.position);

                _camController.MoveTowardsPosition(targetPos, 20f);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            await waitForGroggy;
            _camEffect.PlayShake(_shakeDuration, _shakeStrength);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (_camController != null)
            {
                _camController.IsFollowingPlayer = true;
            }

            InputHandler.UnblockInput();
        }
    }
}
