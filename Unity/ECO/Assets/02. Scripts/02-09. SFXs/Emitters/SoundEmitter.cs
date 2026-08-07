using UnityEngine;
using VInspector;

/// <summary>
/// 월드 공간에 존재하는 오브젝트의 SFX 볼륨을 뷰포트 기준으로 실시간 제어하고,
/// 플레이어와의 X축 거리를 기반으로 스테레오 패닝을 적용하여 방향감을 부여한다.
///
/// 볼륨 결정 규칙:
///   - 화면(뷰포트 0~1) 안: 위치 무관하게 baseVolume × sfxVolume 100% 유지
///   - 화면 밖: 가장자리로부터의 거리에 따라 falloffCurve로 감쇠, falloffRange 거리에서 볼륨 0
///   - isAlwaysAudible 체크 시: 화면 밖 감쇠 없이 항상 baseVolume × sfxVolume 유지
///
/// 패닝 결정 규칙:
///   - 소스가 플레이어 기준 panRange만큼 오른쪽이면 +1, 왼쪽이면 -1
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

    [Header("Pan")]
    // 플레이어로부터 이 거리(월드 유닛)만큼 X축으로 벗어나면 panStereo가 ±1에 도달한다.
    [SerializeField]
    private float _panRange = 15f;

    private AudioSource _audioSource;
    private Camera _cachedCamera;
    private Transform _playerTransform;
    // 매 프레임 SoundManager를 폴링하지 않도록 캐싱하고, 변경 통지를 구독해 갱신한다.
    private float _sfxVolume = 1f;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _cachedCamera = Camera.main;
        if (SoundManager.HasInstance)
        {
            _sfxVolume = SoundManager.Instance.SfxVolume;
            SoundManager.Instance.OnSfxVolumeChanged += SetSfxVolume;
        }
        TryFindPlayer();
        UpdateVolume();
    }

    private void LateUpdate()
    {
        TryFindPlayer();
        UpdateVolume();
    }

    private void OnDisable()
    {
        if (SoundManager.HasInstance)
        {
            SoundManager.Instance.OnSfxVolumeChanged -= SetSfxVolume;
        }
    }

    private void SetSfxVolume(float volume)
    {
        _sfxVolume = volume;
    }

    private void UpdateVolume()
    {
        float attenuation = _isAlwaysAudible ? 1f : CalculateViewportAttenuation();
        _audioSource.volume = _baseVolume * _sfxVolume * attenuation;
        _audioSource.panStereo = CalculatePan();
    }

    // 플레이어를 아직 못 찾은 경우에만 탐색하여 캐싱한다.
    private void TryFindPlayer()
    {
        if (_playerTransform != null)
        {
            return;
        }

        var player = FindFirstObjectByType<PlayerStateMachine>();
        if (player != null)
        {
            _playerTransform = player.transform;
        }
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
        }

        return ViewportAttenuator.Calculate(transform.position, _cachedCamera, _falloffRange, _falloffCurve);
    }

    // 이 오브젝트의 X 위치와 플레이어 X 위치의 차이를 panRange로 정규화하여 -1(좌)~1(우)를 반환한다.
    private float CalculatePan()
    {
        if (_playerTransform == null)
        {
            return 0f;
        }

        float dx = transform.position.x - _playerTransform.position.x;
        return Mathf.Clamp(dx / _panRange, -1f, 1f);
    }
}
