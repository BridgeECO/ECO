using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CameraEffect : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            PlayShake();
        }
    }

    public void PlayShake(float duration = 0.5f, float strength = 1f, int vibrato = 10, float randomness = 90f)
    {
        ShakeCameraAsync(duration, strength, vibrato, randomness).Forget();
    }

    private async UniTask ShakeCameraAsync(float duration = 0.5f, float strength = 1f, int vibrato = 10, float randomness = 90f)
    {
        transform.DOComplete();
        await transform.DOShakePosition(duration, strength, vibrato, randomness, fadeOut: true)
            .SetLink(gameObject)
            .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }
}
