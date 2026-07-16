using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 월드 공간 SFX 전용 AudioSource 풀.
/// 재생 요청 시 유휴 슬롯에 위치를 설정하고 클립을 재생한다.
/// 재생 중인 슬롯은 LateUpdate에서 매 프레임 ViewportAttenuator를 통해 볼륨을 갱신한다.
/// SfxController의 자식 오브젝트에 배치되는 내부 컴포넌트이며, 외부에서 직접 참조하지 않는다.
/// </summary>
public class WorldSfxPool : MonoBehaviour
{
    private readonly List<AudioSource> _sources = new();
    // 재생 중인 슬롯의 월드 위치를 추적하기 위한 병렬 리스트
    private readonly List<Vector3> _sourcePositions = new();

    private Camera _cachedCamera;
    private float _volume = 1f;
    private int _poolSize;
    private float _falloffRange;
    private AnimationCurve _falloffCurve;

    public void Init(int poolSize, float falloffRange, AnimationCurve falloffCurve)
    {
        _poolSize = poolSize;
        _falloffRange = falloffRange;
        _falloffCurve = falloffCurve;
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
            // 초기 볼륨은 재생 전에 감쇠값 적용
            _sources[i].volume = _volume * CalculateAttenuation(worldPos);
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

            float attenuation = CalculateAttenuation(_sourcePositions[i]);
            _sources[i].volume = _volume * attenuation;
        }
    }

    private float CalculateAttenuation(Vector3 worldPos)
    {
        return ViewportAttenuator.Calculate(worldPos, _cachedCamera, _falloffRange, _falloffCurve);
    }
}
