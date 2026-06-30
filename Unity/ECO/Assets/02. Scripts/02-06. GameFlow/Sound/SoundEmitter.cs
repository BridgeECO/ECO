using UnityEngine;
using VInspector;

/// <summary>
/// 월드 공간에 존재하는 오브젝트의 SFX 볼륨을 뷰포트 기준으로 실시간 제어한다.
///
/// 볼륨 결정 규칙:
///   - 화면(뷰포트 0~1) 안: 위치 무관하게 baseVolume × sfxVolume 100% 유지
///   - 화면 밖: 가장자리로부터의 거리에 따라 falloffCurve로 감쇠, falloffRange 거리에서 볼륨 0
///   - isAlwaysAudible 체크 시: 화면 밖 감쇠 없이 항상 baseVolume × sfxVolume 유지
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    [Foldout("Settings")]
    [Header("Volume")]
    [SerializeField]
    private float _baseVolume = 1f;

    [SerializeField]
    private bool _isAlwaysAudible = false;

    [Header("Falloff")]
    [SerializeField]
    private float _falloffRange = 1f;

    /// <summary>뷰포트 가장자리 초과 거리(0~1 정규화)를 볼륨 배율(1~0)로 변환하는 커브.</summary>
    [SerializeField]
    private AnimationCurve _falloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private AudioSource _audioSource;
    private Camera _cachedCamera;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _cachedCamera = Camera.main;
        UpdateVolume();
    }

    private void LateUpdate()
    {
        if (!SoundManager.HasInstance)
        {
            return;
        }

        UpdateVolume();
    }

    private void UpdateVolume()
    {
        float attenuation = _isAlwaysAudible ? 1f : CalculateViewportAttenuation();
        _audioSource.volume = _baseVolume * GetSfxVolume() * attenuation;
    }

    /// <summary>
    /// 뷰포트 좌표 기준으로 화면 밖 거리를 계산하여 볼륨 감쇠 배율(0~1)을 반환한다.
    /// 화면 안이면 1을 반환하고, falloffRange 이상 벗어나면 0을 반환한다.
    /// </summary>
    private float CalculateViewportAttenuation()
    {
        if (_cachedCamera == null)
        {
            _cachedCamera = Camera.main;
            if (_cachedCamera == null)
            {
                return 1f;
            }
        }

        Vector3 vp = _cachedCamera.WorldToViewportPoint(transform.position);

        // 카메라 뒤쪽은 최대 거리로 처리
        if (vp.z < 0f)
        {
            return _falloffCurve.Evaluate(1f);
        }

        float dx = Mathf.Max(0f, -vp.x, vp.x - 1f);
        float dy = Mathf.Max(0f, -vp.y, vp.y - 1f);
        float outOfViewDist = Mathf.Max(dx, dy);

        float normalizedDist = Mathf.Clamp01(outOfViewDist / _falloffRange);
        return _falloffCurve.Evaluate(normalizedDist);
    }

    private float GetSfxVolume()
    {
        return SoundManager.HasInstance ? SoundManager.Instance.SfxVolume : 1f;
    }
}
