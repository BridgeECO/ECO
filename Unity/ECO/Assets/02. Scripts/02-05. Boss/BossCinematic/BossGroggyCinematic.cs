using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VInspector;

public class BossGroggyCinematic : BossCinematicBase
{
    [Foldout("Cinematic Settings")]
    [SerializeField, Tooltip("카메라를 흔드는 시간")]
    private float _shakeDuration = 1.5f;
    [SerializeField, Tooltip("카메라 흔들림 강도")]
    private float _shakeStrength = 1f;

    public override async UniTask PlayCinematicAsync(BossBase boss, CancellationToken cancellationToken)
    {
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            this.GetCancellationTokenOnDestroy());

        boss.StartGroggy();

        if (_camEffect == null)
        {
            return;
        }

        await _camEffect.ShakeCameraAsync(
            _shakeDuration,
            _shakeStrength,
            cancellationToken: linkedCts.Token);

    }
}
