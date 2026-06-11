using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VInspector;

public class BossGroggyCinematic : BossCinematicBase
{
    [Foldout("Cinematic Settings")]
    [SerializeField, Tooltip("카메라를 흔드는 시간")]
    private float _shakeDuration = 1.5f;
    [SerializeField, Tooltip("포효 시 카메라 흔들림 강도")]
    private float _shakeStrength = 1f;

    public override async UniTask PlayCinematicAsync(BossBase boss)
    {
        boss.StartGroggy();

        CameraEffect _camEffect = Camera.main.GetComponent<CameraEffect>();

        if (_camEffect == null)
        {
            return;
        }

        await _camEffect.ShakeCameraAsync(_shakeDuration, _shakeStrength);
    }
}
