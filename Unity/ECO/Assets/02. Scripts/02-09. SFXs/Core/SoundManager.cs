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
    private AudioClip[] _bgmClips;

    [SerializeField]
    private AudioClip[] _sfxClips;

    private float _sfxVolume = 1f;

    /// <summary>SoundEmitter가 매 프레임 참조하는 SFX 전역 볼륨 배율.</summary>
    public float SfxVolume => _sfxVolume;

    protected override void Awake()
    {
        base.Awake();
        LoadSavedVolumes();
        ValidateSfxClipsArray();
    }

    // ──────────────────────────────────────────
    // BGM Control API
    // ──────────────────────────────────────────

    public void PlayBgm(EBgmType type)
    {
        int index = (int)type;
        if (_bgmClips == null || index < 0 || index >= _bgmClips.Length)
        {
            return;
        }

        AudioClip clip = _bgmClips[index];
        if (clip != null)
        {
            _bgmController.Play(clip, BGM_FADE_DURATION);
        }
    }

    public void StopBgm()
    {
        _bgmController.Stop(BGM_FADE_DURATION);
    }

    // ──────────────────────────────────────────
    // SFX Control API (MonoBehaviour & 2D)
    // ──────────────────────────────────────────

    public void PlayPlayerSfx(AudioClip clip)
    {
        _sfxController.PlayPlayerSfx(clip);
    }

    /// <summary>ESfxClip Enum으로 Player SFX를 재생한다.</summary>
    public void PlayPlayerSfx(ESfxClip clip)
    {
        AudioClip audioClip = GetSfxClip(clip);
        if (audioClip != null)
        {
            _sfxController.PlayPlayerSfx(audioClip);
        }
    }

    public void PlayUiSfx(AudioClip clip)
    {
        _sfxController.PlayUiSfx(clip);
    }

    /// <summary>ESfxClip Enum으로 UI SFX를 재생한다.</summary>
    public void PlayUiSfx(ESfxClip clip)
    {
        AudioClip audioClip = GetSfxClip(clip);
        if (audioClip != null)
        {
            _sfxController.PlayUiSfx(audioClip);
        }
    }

    /// <summary>
    /// 동시 재생 가능한 Player SFX 슬롯 수를 변경한다.
    /// 인게임 상황(이동 중 점프, 공격 등 동시 액션 수)에 따라 실시간으로 호출한다.
    /// </summary>
    public void ChangePlayerSfxPoolSize(int activeCount)
    {
        _sfxController.SetMaxPlayerSfxCount(activeCount);
    }

    // ──────────────────────────────────────────
    // Volume Control API
    // ──────────────────────────────────────────

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

    // ──────────────────────────────────────────
    // 내부 헬퍼 메서드
    // ──────────────────────────────────────────

    /// <summary>게임 시작 시 PlayerPrefs에 저장된 볼륨 설정을 즉시 적용한다.</summary>
    private void LoadSavedVolumes()
    {
        SetMasterVolume(PlayerPrefs.GetFloat("Settings_Sound_Master", 1f));
        SetBgmVolume(PlayerPrefs.GetFloat("Settings_Sound_Bgm", 1f));
        SetSfxVolume(PlayerPrefs.GetFloat("Settings_Sound_Sfx", 1f));
    }

    /// <summary>
    /// ESfxClip 인덱스로 AudioClip을 조회한다.
    /// 배열 크기 미스매치 및 null 슬롯은 null을 반환하고 호출부에서 무시한다.
    /// </summary>
    private AudioClip GetSfxClip(ESfxClip clip)
    {
        int index = (int)clip;
        if (_sfxClips == null || index < 0 || index >= _sfxClips.Length)
        {
            return null;
        }

        return _sfxClips[index];
    }

    /// <summary>Awake 시점에 배열 크기와 Enum 항목 수의 불일치를 경고한다.</summary>
    private void ValidateSfxClipsArray()
    {
        int enumCount = System.Enum.GetValues(typeof(ESfxClip)).Length;
        if (enumCount == 0)
        {
            return;
        }

        if (_sfxClips == null || _sfxClips.Length != enumCount)
        {
            Debug.LogWarning(
                $"[SoundManager] _sfxClips 배열 크기({_sfxClips?.Length ?? 0})가 " +
                $"ESfxClip 항목 수({enumCount})와 다릅니다. " +
                "Tools > Sound > Generate ESfxClip Enum 재실행 후 Inspector에서 재할당하세요.");
        }
    }
}
