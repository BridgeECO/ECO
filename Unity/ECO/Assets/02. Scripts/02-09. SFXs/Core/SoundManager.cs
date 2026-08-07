using UnityEngine;
using VInspector;

/// <summary>
/// 사운드 시스템의 퍼사드 싱글톤. 외부 코드가 사운드 재생·제어 시 유일하게 접근하는 진입점.
/// BGM 제어는 BgmController에, SFX 제어는 SfxController에 위임한다.
/// 씬 루트의 전용 GameObject에 배치하고 isDontDestroyOnLoad를 활성화한다.
/// </summary>
public class SoundManager : MonoBehaviourSingleton<SoundManager>
{
    // SoundEmitter 등이 매 프레임 폴링하는 대신 구독으로 볼륨 변화를 통지받는다.
    public System.Action<float> OnSfxVolumeChanged;

    private const float BGM_FADE_DURATION = 1.0f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private BgmController _bgmController;

    [SerializeField]
    private SfxController _sfxController;

    [Foldout("Project")]
    [SerializeField]
    private SoundLibrary _soundLibrary = new();

    private float _sfxVolume = 1f;
    private EBgmType? _currentBgmType;

    // SFX 전역 볼륨 배율. 실시간 갱신은 OnSfxVolumeChanged 구독으로 통지받는다.
    public float SfxVolume => _sfxVolume;

    // 월드 SFX 감쇠·거리 계산의 기준점(플레이어). 씬 로드 후 플레이어가 스스로 등록한다.
    // 오디오 시스템이 플레이어 구체 타입을 탐색(Find)하지 않기 위한 수용점이다.
    public Transform ListenerTarget { get; private set; }

    public void SetListener(Transform listenerTarget)
    {
        ListenerTarget = listenerTarget;
    }

    protected override void Awake()
    {
        base.Awake();
        LoadSavedVolumes();
        _soundLibrary.Initialize();
    }

    public void PlayBgm(EBgmType type)
    {
        if (_bgmController == null || _currentBgmType == type)
        {
            return;
        }

        AudioClip clip = _soundLibrary.GetBgmClip(type);
        if (clip != null)
        {
            _currentBgmType = type;
            _bgmController.Play(clip, BGM_FADE_DURATION);
        }
    }

    public void StopBgm()
    {
        _currentBgmType = null;
        _bgmController.Stop(BGM_FADE_DURATION);
    }

    // ESfxClip을 로드한 뒤 널 체크를 거쳐 특정 액션을 실행하는 공통 헬퍼 메서드
    private void ExecuteWithClip(ESfxClip clip, System.Action<AudioClip> action)
    {
        AudioClip audioClip = GetSfxClip(clip);
        if (audioClip != null)
        {
            action?.Invoke(audioClip);
        }
    }

    // ESfxClip Enum으로 Player SFX를 재생한다.
    public void PlayPlayerSfx(ESfxClip clip)
    {
        ExecuteWithClip(clip, _sfxController.PlayPlayerSfx);
    }

    // 재생 중인 Player SFX를 강제 정지한다.
    public void StopPlayerSfx(ESfxClip clip)
    {
        ExecuteWithClip(clip, _sfxController.StopPlayerSfx);
    }

    // ESfxClip Enum으로 Player SFX를 루프 재생한다.
    public void PlayPlayerLoopSfx(ESfxClip clip)
    {
        ExecuteWithClip(clip, audioClip => _sfxController.PlayLoopSfx(clip, audioClip));
    }

    // 재생 중인 루프 SFX를 정지한다.
    public void StopPlayerLoopSfx(ESfxClip clip)
    {
        _sfxController.StopLoopSfx(clip);
    }

    // ESfxClip Enum으로 UI SFX를 재생한다.
    public void PlayUiSfx(ESfxClip clip)
    {
        ExecuteWithClip(clip, _sfxController.PlayUiSfx);
    }

    // 월드 공간에 위치한 오브젝트의 SFX를 재생한다.
    // 화면 밖 거리에 따라 볼륨이 자동 감쇠되어 공간감을 부여한다.
    public void PlayWorldSfx(ESfxClip clip, Transform source)
    {
        ExecuteWithClip(clip, audioClip => _sfxController.PlayWorldSfx(audioClip, source.position));
    }

    // 오브젝트가 직접 소유한 AudioSource에 ESfxClip을 1회 재생한다.
    // 보스·에너지코어처럼 씬에 고정된 오브젝트에서 WorldSfxPool 없이 위치 기반 재생이 필요할 때 사용한다.
    public void PlaySfxOnSource(ESfxClip clip, AudioSource source)
    {
        ExecuteWithClip(clip, audioClip => {
            if (source != null)
            {
                source.PlayOneShot(audioClip, _sfxVolume);
            }
        });
    }

    // 오브젝트가 직접 소유한 AudioSource에 ESfxClip을 루프 재생한다.
    public void PlayLoopSfxOnSource(ESfxClip clip, AudioSource source)
    {
        ExecuteWithClip(clip, audioClip => {
            if (source == null)
            {
                return;
            }

            // 이미 동일한 클립이 루프 재생 중이면 중복 재생 방지
            if (source.clip == audioClip && source.isPlaying && source.loop)
            {
                return;
            }

            source.clip = audioClip;
            source.loop = true;
            source.volume = _sfxVolume;
            source.Play();
        });
    }

    // 오브젝트가 직접 소유한 AudioSource의 루프 재생을 정지한다.
    public void StopLoopSfxOnSource(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.Stop();
        source.clip = null;
    }

    // 동시 재생 가능한 Player SFX 슬롯 수를 변경한다.
    // 인게임 상황(이동 중 점프, 공격 등 동시 액션 수)에 따라 실시간으로 호출한다.
    public void ChangePlayerSfxPoolSize(int activeCount)
    {
        _sfxController.SetMaxPlayerSfxCount(activeCount);
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }

    public void SetBgmVolume(float volume)
    {
        _bgmController.SetVolume(volume);
    }

    public void SetSfxVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        _sfxController.SetVolume(_sfxVolume);
        OnSfxVolumeChanged?.Invoke(_sfxVolume);
    }

    // 게임 시작 시 PlayerPrefs에 저장된 볼륨 설정을 즉시 적용한다.
    private void LoadSavedVolumes()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(SoundVolumeKeys.MASTER, 1f));
        SetBgmVolume(PlayerPrefs.GetFloat(SoundVolumeKeys.BGM, 1f));
        SetSfxVolume(PlayerPrefs.GetFloat(SoundVolumeKeys.SFX, 1f));
    }


    // ESfxClip 인덱스로 AudioClip을 조회한다.
    private AudioClip GetSfxClip(ESfxClip clip)
    {
        if (_soundLibrary is null)
        {
            return null;
        }

        return _soundLibrary.GetSfxClip(clip);
    }
}
