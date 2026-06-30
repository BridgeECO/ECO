using UnityEngine;
using VInspector;

/// <summary>
/// 사운드 시스템의 퍼사드 싱글톤. 외부 코드가 사운드를 재생·제어하는 유일한 진입점이다.
/// BGM 제어는 BgmController에, SFX 풀 제어는 SfxController에 위임한다.
/// 씬 루트의 전용 GameObject에 배치하고 isDontDestroyOnLoad를 활성화한다.
/// </summary>
public class SoundManager : MonoBehaviourSingleton<SoundManager>
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private BgmController _bgmController;

    [SerializeField]
    private SfxController _sfxController;

    [Foldout("Project")]
    [Header("BGM Clips")]
    [SerializeField]
    private AudioClip[] _bgmClips;

    private float _sfxVolume = 1f;

    /// <summary>SoundEmitter가 매 프레임 참조하는 SFX 전역 볼륨 배율.</summary>
    public float SfxVolume => _sfxVolume;

    protected override void Awake()
    {
        base.Awake();
        LoadSavedVolumes();
    }

    // ──────────────────────────────────────────
    // BGM
    // ──────────────────────────────────────────

    public void PlayBgm(EBgmType type, float fadeDuration = 1f)
    {
        int index = (int)type;
        if (_bgmClips == null || index < 0 || _bgmClips.Length <= index || _bgmClips[index] == null)
        {
            return;
        }

        _bgmController.Play(_bgmClips[index], fadeDuration);
    }

    public void StopBgm(float fadeDuration = 1f)
    {
        _bgmController.Stop(fadeDuration);
    }

    // ──────────────────────────────────────────
    // SFX
    // ──────────────────────────────────────────

    /// <summary>플레이어 SFX를 중앙(2D) 고정 위치에서 재생한다.</summary>
    public void PlayPlayerSfx(AudioClip clip)
    {
        _sfxController.PlayPlayerSfx(clip);
    }

    public void PlayUiSfx(AudioClip clip)
    {
        _sfxController.PlayUiSfx(clip);
    }

    /// <summary>
    /// 동시 재생 가능한 Player SFX 슬롯 수를 변경한다.
    /// 인게임 상황(이동 중 점프, 공격 등 동시 액션 수)에 따라 실시간으로 호출한다.
    /// </summary>
    public void SetMaxPlayerSfxCount(int count)
    {
        _sfxController.SetMaxPlayerSfxCount(count);
    }

    // ──────────────────────────────────────────
    // 볼륨 제어 (UI_SettingsTab_Sound에서 호출)
    // ──────────────────────────────────────────

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }

    public void SetBgmVolume(float volume)
    {
        _bgmController.SetVolume(Mathf.Clamp01(volume));
    }

    public void SetSfxVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        _sfxController.SetVolume(_sfxVolume);
        // SoundEmitter들은 SfxVolume 프로퍼티를 참조하므로 별도 알림 불필요
    }

    // ──────────────────────────────────────────
    // 내부 초기화
    // ──────────────────────────────────────────

    /// <summary>게임 시작 시 PlayerPrefs에 저장된 볼륨 설정을 즉시 적용한다.</summary>
    private void LoadSavedVolumes()
    {
        SetMasterVolume(PlayerPrefs.GetFloat("Settings_Sound_Master", 1f));
        SetBgmVolume(PlayerPrefs.GetFloat("Settings_Sound_Bgm", 1f));
        SetSfxVolume(PlayerPrefs.GetFloat("Settings_Sound_Sfx", 1f));
    }
}
