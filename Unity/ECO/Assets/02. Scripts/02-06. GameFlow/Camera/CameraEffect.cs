using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CameraEffect : MonoBehaviour
{
    private Camera _mainCamera;
    private Tweener _zoomTween;

    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
    }

    public void PlayShake(float duration = 0.5f, float strength = 1f, int vibrato = 10, float randomness = 90f)
    {
        ShakeCameraAsync(duration, strength, vibrato, randomness).Forget();
    }

    public async UniTask ShakeCameraAsync(float duration, float strength, int vibrato = 10, float randomness = 90f)
    {
        transform.DOComplete();
        await transform.DOShakePosition(duration, strength, vibrato, randomness, fadeOut: true)
            .SetLink(gameObject)
            .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

        transform.localPosition = Vector3.zero;
    }

    public void PlayZoom(float targetSize = 5f, float duration = 5f)
    {
        PlayZoomAsync(targetSize, duration).Forget();
    }

    public async UniTask PlayZoomAsync(float targetSize, float duration, Ease ease = Ease.OutCubic)
    {
        _zoomTween?.Kill();

        _zoomTween = _mainCamera.DOOrthoSize(targetSize, duration)
            .SetEase(ease)
            .SetLink(gameObject);

        await _zoomTween.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

        transform.localPosition = Vector3.zero;
    }
}
