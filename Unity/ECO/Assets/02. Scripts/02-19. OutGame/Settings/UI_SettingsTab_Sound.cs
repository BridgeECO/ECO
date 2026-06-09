using UnityEngine;
using VInspector;

public class UI_SettingsTab_Sound : UI_SettingsTabBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_ClickSlider _masterVolumeSlider;

    [SerializeField]
    private UI_ClickSlider _bgmVolumeSlider;

    [SerializeField]
    private UI_ClickSlider _sfxVolumeSlider;

    [SerializeField]
    private UI_ClickSlider _voiceVolumeSlider;

    private bool _isInitialized;

    private float _currentMasterVolume;
    private float _currentBgmVolume;
    private float _currentSfxVolume;
    private float _currentVoiceVolume;

    private const string PrefKey_MasterVolume = "Settings_Sound_Master";
    private const string PrefKey_BgmVolume = "Settings_Sound_Bgm";
    private const string PrefKey_SfxVolume = "Settings_Sound_Sfx";
    private const string PrefKey_VoiceVolume = "Settings_Sound_Voice";

    private void OnEnable()
    {
        if (_isInitialized)
        {
            SyncUIToCurrentValues();
        }
    }

    private void OnDisable()
    {
        // 팝업이 닫힐 때(적용 버튼을 누르지 않고 무시했을 경우 등) 원래의 저장된 상태로 복원
        if (IsDirty)
        {
            LoadSavedSettings();
            ApplySettingsImmediately();
            IsDirty = false;
        }
    }

    private void Awake()
    {
        InitTab();
    }

    public override void InitTab()
    {
        if (_isInitialized)
        {
            return;
        }

        LoadSavedSettings();
        SyncUIToCurrentValues();

        if (_masterVolumeSlider != null) _masterVolumeSlider.OnValueChanged += val => { _currentMasterVolume = val; ApplySettingsImmediately(); IsDirty = true; };
        if (_bgmVolumeSlider != null) _bgmVolumeSlider.OnValueChanged += val => { _currentBgmVolume = val; ApplySettingsImmediately(); IsDirty = true; };
        if (_sfxVolumeSlider != null) _sfxVolumeSlider.OnValueChanged += val => { _currentSfxVolume = val; ApplySettingsImmediately(); IsDirty = true; };
        if (_voiceVolumeSlider != null) _voiceVolumeSlider.OnValueChanged += val => { _currentVoiceVolume = val; ApplySettingsImmediately(); IsDirty = true; };

        _isInitialized = true;
    }

    private void LoadSavedSettings()
    {
        _currentMasterVolume = PlayerPrefs.GetFloat(PrefKey_MasterVolume, 1f);
        _currentBgmVolume = PlayerPrefs.GetFloat(PrefKey_BgmVolume, 1f);
        _currentSfxVolume = PlayerPrefs.GetFloat(PrefKey_SfxVolume, 1f);
        _currentVoiceVolume = PlayerPrefs.GetFloat(PrefKey_VoiceVolume, 1f);
    }

    private void SyncUIToCurrentValues()
    {
        if (_masterVolumeSlider != null) _masterVolumeSlider.SetValueWithoutNotify(_currentMasterVolume);
        if (_bgmVolumeSlider != null) _bgmVolumeSlider.SetValueWithoutNotify(_currentBgmVolume);
        if (_sfxVolumeSlider != null) _sfxVolumeSlider.SetValueWithoutNotify(_currentSfxVolume);
        if (_voiceVolumeSlider != null) _voiceVolumeSlider.SetValueWithoutNotify(_currentVoiceVolume);
    }

    private void ApplySettingsImmediately()
    {
        // 실제 사운드 매니저와 연동하는 코드 작성 필요
    }

    public override void RefreshTab()
    {
        // 열릴 때 현재 상태와 UI 동기화 (저장되지 않은 변경사항을 버리지 않도록 LoadSavedSettings 제외)
        SyncUIToCurrentValues();
    }

    public override void ResetTabToDefault()
    {
        _currentMasterVolume = 1f;
        _currentBgmVolume = 1f;
        _currentSfxVolume = 1f;
        _currentVoiceVolume = 1f;

        SyncUIToCurrentValues();
        ApplySettingsImmediately();
        IsDirty = true;
    }

    public override void SaveTabSettings()
    {
        PlayerPrefs.SetFloat(PrefKey_MasterVolume, _currentMasterVolume);
        PlayerPrefs.SetFloat(PrefKey_BgmVolume, _currentBgmVolume);
        PlayerPrefs.SetFloat(PrefKey_SfxVolume, _currentSfxVolume);
        PlayerPrefs.SetFloat(PrefKey_VoiceVolume, _currentVoiceVolume);
        PlayerPrefs.Save();

        IsDirty = false;
    }

}
