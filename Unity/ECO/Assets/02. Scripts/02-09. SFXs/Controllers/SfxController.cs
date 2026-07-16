using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// Player SFX 풀과 UI SFX 재생을 담당한다.
/// Player SFX는 슬롯 기반 풀로 최대 동시 재생 수를 제어하며,
/// UI SFX는 단일 AudioSource에서 PlayOneShot으로 재생한다.
/// SoundManager의 자식 오브젝트에 배치되는 내부 컴포넌트이며, 외부에서 직접 참조하지 않는다.
/// </summary>
public class SfxController : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private AudioSource _uiSource;

    [Foldout("Settings")]
    [Header("Player SFX Pool")]
    [SerializeField]
    private int _playerPoolSize = 8;

    [SerializeField]
    private int _defaultMaxPlayerSfxCount = 4;

    private readonly List<AudioSource> _playerSources = new List<AudioSource>();
    private readonly Dictionary<int, AudioSource> _loopingSources = new();
    private float _volume = 1f;
    private int _maxPlayerSfxCount;

    private void Awake()
    {
        _maxPlayerSfxCount = _defaultMaxPlayerSfxCount;
        InitPlayerPool();
        InitUiSource();
    }

    /// <summary>플레이어 SFX를 유휴 슬롯에서 재생한다. 모든 슬롯이 사용 중이면 무시한다.</summary>
    public void PlayPlayerSfx(AudioClip clip)
    {
        for (int i = 0; i < _maxPlayerSfxCount && i < _playerSources.Count; i++)
        {
            if (_playerSources[i].isPlaying)
            {
                continue;
            }

            _playerSources[i].clip = clip;
            _playerSources[i].Play();
            return;
        }
    }

    /// <summary>재생 중인 특정 플레이어 SFX를 강제 정지한다.</summary>
    public void StopPlayerSfx(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        for (int i = 0; i < _playerSources.Count; i++)
        {
            if (_playerSources[i].isPlaying && _playerSources[i].clip == clip)
            {
                _playerSources[i].Stop();
            }
        }
    }

    public void PlayUiSfx(AudioClip clip)
    {
        _uiSource.PlayOneShot(clip);
    }

    /// <summary>플레이어 SFX를 루프 모드로 재생한다. 이미 재생 중인 경우 무시한다.</summary>
    public void PlayLoopSfx(ESfxClip clip, AudioClip audioClip)
    {
        int key = (int)clip;
        if (_loopingSources.TryGetValue(key, out var source))
        {
            if (source != null)
            {
                if (!source.isPlaying)
                {
                    source.clip = audioClip;
                    source.Play();
                }
                return;
            }
        }

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.spatialBlend = 0f;
        newSource.loop = true;
        newSource.playOnAwake = false;
        newSource.volume = _volume;
        newSource.clip = audioClip;
        newSource.Play();

        _loopingSources[key] = newSource;
    }

    /// <summary>재생 중인 루프 SFX를 정지한다.</summary>
    public void StopLoopSfx(ESfxClip clip)
    {
        int key = (int)clip;
        if (_loopingSources.TryGetValue(key, out var source))
        {
            if (source != null)
            {
                source.Stop();
            }
        }
    }

    public void SetVolume(float volume)
    {
        _volume = volume;
        _uiSource.volume = _volume;

        for (int i = 0; i < _playerSources.Count; i++)
        {
            _playerSources[i].volume = _volume;
        }

        foreach (var source in _loopingSources.Values)
        {
            if (source != null)
            {
                source.volume = _volume;
            }
        }
    }

    private void OnDestroy()
    {
        _loopingSources.Clear();
    }

    /// <summary>
    /// 동시 재생 가능한 Player SFX 최대 슬롯 수를 변경한다.
    /// 인게임 상황에 따라 실시간으로 호출될 수 있다.
    /// </summary>
    public void SetMaxPlayerSfxCount(int count)
    {
        _maxPlayerSfxCount = Mathf.Clamp(count, 1, _playerPoolSize);
    }

    // Awake에서만 호출되므로 AddComponent는 초기화 시 1회성 처리
    private void InitPlayerPool()
    {
        for (int i = 0; i < _playerPoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.spatialBlend = 0f;
            source.loop = false;
            source.playOnAwake = false;
            source.volume = _volume;
            _playerSources.Add(source);
        }
    }

    private void InitUiSource()
    {
        _uiSource.spatialBlend = 0f;
        _uiSource.loop = false;
        _uiSource.playOnAwake = false;
        _uiSource.volume = _volume;
    }
}
