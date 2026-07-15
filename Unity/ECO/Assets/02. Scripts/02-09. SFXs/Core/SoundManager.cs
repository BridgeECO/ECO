using UnityEngine;
using VInspector;

/// <summary>
/// 사운드 시스템의 퍼사드 싱글톤. 외부 코드가 사운드 재생·제어 시 유일하게 접근하는 진입점.
/// BGM 제어는 BgmController에, SFX 제어는 SfxController에 위임한다.
/// 씬 루트의 전용 GameObject에 배치하고 isDontDestroyOnLoad를 활성화한다.
/// </summary>
public class SoundManager : MonoBehaviourSingleton<SoundManager>
{
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

    // SoundEmitter가 매 프레임 참조하는 SFX 전역 볼륨 배율.
    public float SfxVolume => _sfxVolume;

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

    // ESfxClip Enum으로 Player SFX를 재생한다.
    public void PlayPlayerSfx(ESfxClip clip)
    {
        AudioClip audioClip = GetSfxClip(clip);
        if (audioClip == null)
        {
            return;
        }
        _sfxController.PlayPlayerSfx(audioClip);
    }

    // ESfxClip Enum으로 Player SFX를 루프 재생한다.
    public void PlayPlayerLoopSfx(ESfxClip clip)
    {
        AudioClip audioClip = GetSfxClip(clip);
        if (audioClip == null)
        {
            return;
        }
        _sfxController.PlayLoopSfx(clip, audioClip);
    }

    // 재생 중인 루프 SFX를 정지한다.
    public void StopPlayerLoopSfx(ESfxClip clip)
    {
        _sfxController.StopLoopSfx(clip);
    }

    // ESfxClip Enum으로 UI SFX를 재생한다.
    public void PlayUiSfx(ESfxClip clip)
    {
        AudioClip audioClip = GetSfxClip(clip);
        if (audioClip == null)
        {
            return;
        }
        _sfxController.PlayUiSfx(audioClip);
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
    }

    // 게임 시작 시 PlayerPrefs에 저장된 볼륨 설정을 즉시 적용한다.
    private void LoadSavedVolumes()
    {
        SetMasterVolume(PlayerPrefs.GetFloat("Settings_Sound_Master", 1f));
        SetBgmVolume(PlayerPrefs.GetFloat("Settings_Sound_Bgm", 1f));
        SetSfxVolume(PlayerPrefs.GetFloat("Settings_Sound_Sfx", 1f));
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
