using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 월드 공간 SFX 전용 AudioSource 풀.
/// 재생 요청 시 유휴 슬롯에 위치를 설정하고 클립을 재생한다.
/// 재생 중인 슬롯은 LateUpdate에서 매 프레임 ViewportAttenuator로 볼륨을 갱신하고,
/// 플레이어 기준 X축 오프셋으로 panStereo를 갱신하여 방향감을 부여한다.
/// SfxController의 자식 오브젝트에 배치되는 내부 컴포넌트이며, 외부에서 직접 참조하지 않는다.
/// </summary>
public class WorldSfxPool : MonoBehaviour
{
    private readonly List<AudioSource> _sources = new();
    // 재생 중인 슬롯의 월드 위치를 추적하기 위한 병렬 리스트
    private readonly List<Vector3> _sourcePositions = new();

    private Camera _cachedCamera;
    private Transform _playerTransform;
    private float _volume = 1f;
    private int _poolSize;
    private float _falloffRange;
    private AnimationCurve _falloffCurve;
    // 플레이어로부터 이 거리만큼 X축으로 벗어나면 panStereo가 ±1에 도달한다.
    private float _panRange;

    public void Init(int poolSize, float falloffRange, AnimationCurve falloffCurve, float panRange)
    {
        _poolSize = poolSize;
        _falloffRange = falloffRange;
        _falloffCurve = falloffCurve;
        _panRange = panRange;
        _cachedCamera = Camera.main;

        for (int i = 0; i < _poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.spatialBlend = 0f;
            source.loop = false;
            source.playOnAwake = false;
            source.volume = _volume;
            _sources.Add(source);
            _sourcePositions.Add(Vector3.zero);
        }
    }

    /// <summary>worldPos 위치에서 clip을 1회 재생한다. 유휴 슬롯이 없으면 무시한다.</summary>
    public void Play(AudioClip clip, Vector3 worldPos)
    {
        for (int i = 0; i < _sources.Count; i++)
        {
            if (_sources[i].isPlaying)
            {
                continue;
            }

            _sourcePositions[i] = worldPos;
            _sources[i].clip = clip;
            _sources[i].volume = _volume * CalculateAttenuation(worldPos);
            _sources[i].panStereo = CalculatePan(worldPos);
            _sources[i].Play();
            return;
        }
    }

    public void SetVolume(float volume)
    {
        _volume = volume;
    }

    private void LateUpdate()
    {
        TryFindPlayer();

        if (_cachedCamera == null)
        {
            _cachedCamera = Camera.main;
        }

        for (int i = 0; i < _sources.Count; i++)
        {
            if (!_sources[i].isPlaying)
            {
                continue;
            }

            _sources[i].volume = _volume * CalculateAttenuation(_sourcePositions[i]);
            _sources[i].panStereo = CalculatePan(_sourcePositions[i]);
        }
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

    private float CalculateAttenuation(Vector3 worldPos)
    {
        return ViewportAttenuator.Calculate(worldPos, _cachedCamera, _falloffRange, _falloffCurve);
    }

    // 소스의 X 위치와 플레이어 X 위치의 차이를 panRange로 정규화하여 -1(좌)~1(우)를 반환한다.
    private float CalculatePan(Vector3 worldPos)
    {
        if (_playerTransform == null)
        {
            return 0f;
        }

        float dx = worldPos.x - _playerTransform.position.x;
        return Mathf.Clamp(dx / _panRange, -1f, 1f);
    }
}
