using Cysharp.Threading.Tasks;
using DG.Tweening;
using ECO;
using NPOI.SS.Formula.Functions;
using UnityEngine;
using VInspector;

public class EndChasingCinematic : BossCinematicBase
{
    [Foldout("Cinematic Settings")]
    [SerializeField, Tooltip("보스에게 카메라가 이동하는 시간")]
    private float _panToBossDuration = 0.8f;
    [SerializeField, Tooltip("카메라를 흔드는 시간")]
    private float _shakeDuration = 1.5f;
    [SerializeField, Tooltip("포효 시 카메라 흔들림 강도")]
    private float _shakeStrength = 1f;
    [SerializeField, Tooltip("플레이어에게 다시 돌아오는 시간")]
    private float _returnDuration = 1.0f;

    public override async UniTask PlayCinematicAsync(BossBase boss)
    {
        CameraController _camController = Camera.main.GetComponent<CameraController>();
        CameraEffect _camEffect = Camera.main.GetComponent<CameraEffect>();
        var cts = this.GetCancellationTokenOnDestroy();

        if (_camController == null || _camEffect == null)
        {
            return;
        }

        InputHandler.BlockInput();
        _camController.IsFollowingPlayer = false;

        bool isGroggy = false;
        boss.WaitForStateAsync(EBossState.Groggy).ContinueWith(() => isGroggy = true).Forget();

        while (!isGroggy)
        {
            Vector3 targetPos = _camController.GetClampedPosition(boss.transform.position);

            Camera.main.transform.position = Vector3.MoveTowards(
                Camera.main.transform.position,
                targetPos,
                20f * Time.deltaTime
            );

            await UniTask.Yield(PlayerLoopTiming.Update, cts);
        }

        await _camEffect.ShakeCameraAsync(_shakeDuration, _shakeStrength);

        _camController.IsFollowingPlayer = true;

        InputHandler.UnblockInput();
    }
}
