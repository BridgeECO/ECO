using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// Player SFX 풀, World SFX 풀, UI SFX 재생을 담당한다.
/// Player SFX는 슬롯 기반 풀로 최대 동시 재생 수를 제어하며,
/// World SFX는 WorldSfxPool이 위치 기반 볼륨 감쇠·방향 패닝을 적용하고,
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

    [Header("World SFX Pool")]
    [SerializeField]
    private int _worldPoolSize = 4;

    // 화면 가장자리를 기준으로 이 배율만큼 벗어났을 때 볼륨이 0이 된다. (뷰포트 비율 기준)
    [SerializeField]
    private float _worldFalloffRange = 1f;

    // 플레이어로부터 이 거리(월드 유닛)만큼 X축으로 벗어나면 패닝이 최대(-1 또는 1)가 된다.
    [SerializeField]
    private float _worldPanRange = 15f;

    [SerializeField]
    private AnimationCurve _worldFalloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private readonly List<AudioSource> _playerSources = new List<AudioSource>();
    private WorldSfxPool _worldSfxPool;
    private LoopSfxPool _loopSfxPool;
    private float _volume = 1f;
    private int _maxPlayerSfxCount;

    private void Awake()
    {
        _maxPlayerSfxCount = _defaultMaxPlayerSfxCount;
        InitPlayerPool();
        InitWorldPool();
        InitLoopPool();
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

    /// <summary>worldPos 위치에서 월드 SFX를 재생한다. 화면 밖 거리에 따라 볼륨이 자동 감쇠되고 방향 패닝이 적용된다.</summary>
    public void PlayWorldSfx(AudioClip clip, Vector3 worldPos)
    {
        _worldSfxPool.Play(clip, worldPos);
    }

    /// <summary>플레이어 SFX를 루프 모드로 재생한다. 이미 재생 중인 경우 무시한다.</summary>
    public void PlayLoopSfx(ESfxClip clip, AudioClip audioClip)
    {
        _loopSfxPool.Play(clip, audioClip);
    }

    /// <summary>재생 중인 루프 SFX를 정지한다.</summary>
    public void StopLoopSfx(ESfxClip clip)
    {
        _loopSfxPool.Stop(clip);
    }

    public void SetVolume(float volume)
    {
        _volume = volume;
        _uiSource.volume = _volume;

        for (int i = 0; i < _playerSources.Count; i++)
        {
            _playerSources[i].volume = _volume;
        }

        _loopSfxPool.SetVolume(_volume);
        _worldSfxPool.SetVolume(_volume);
    }

    /// <summary>
    /// 동시 재생 가능한 Player SFX 최대 슬롯 수를 변경한다.
    /// 인게임 상황에 따라 실시간으로 호출될 수 있다.
    /// </summary>
    public void SetMaxPlayerSfxCount(int count)
    {
        _maxPlayerSfxCount = Mathf.Clamp(count, 1, _playerPoolSize);
    }

    // Awake에서만 호출되므로 AddComponent/컴포넌트 생성은 초기화 시 1회성 처리
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

    private void InitWorldPool()
    {
        _worldSfxPool = gameObject.AddComponent<WorldSfxPool>();
        _worldSfxPool.Init(_worldPoolSize, _worldFalloffRange, _worldFalloffCurve, _worldPanRange);
    }

    private void InitLoopPool()
    {
        _loopSfxPool = gameObject.AddComponent<LoopSfxPool>();
    }

    private void InitUiSource()
    {
        _uiSource.spatialBlend = 0f;
        _uiSource.loop = false;
        _uiSource.playOnAwake = false;
        _uiSource.volume = _volume;
    }
}
