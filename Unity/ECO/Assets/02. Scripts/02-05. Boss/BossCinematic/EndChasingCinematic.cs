using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
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
        var cts = this.GetCancellationTokenOnDestroy();

        if (_camController == null || _camEffect == null)
        {
            return;
        }

        InputHandler.BlockInput();
        _camController.IsFollowingPlayer = false;

        try
        {
            UniTask waitForGroggy = boss.WaitForStateAsync(EBossState.Groggy, cts);
            while (!ReferenceEquals(boss, null) && waitForGroggy.Status == UniTaskStatus.Pending)
            {
                Vector3 targetPos = _camController.GetClampedPosition(boss.transform.position);

                _camController.MoveTowardsPosition(targetPos, 20f);

                await UniTask.Yield(PlayerLoopTiming.Update, cts);
            }

            await waitForGroggy;
            _camEffect.PlayShake(_shakeDuration, _shakeStrength);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _camController.IsFollowingPlayer = true;
            InputHandler.UnblockInput();
        }
    }
}
