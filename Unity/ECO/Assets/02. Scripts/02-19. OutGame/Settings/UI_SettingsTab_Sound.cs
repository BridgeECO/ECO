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

    private bool _isDirty;
    private bool _isInitialized;

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

        if (_masterVolumeSlider != null) _masterVolumeSlider.OnValueChanged += _ => SetDirty();
        if (_bgmVolumeSlider != null) _bgmVolumeSlider.OnValueChanged += _ => SetDirty();
        if (_sfxVolumeSlider != null) _sfxVolumeSlider.OnValueChanged += _ => SetDirty();
        if (_voiceVolumeSlider != null) _voiceVolumeSlider.OnValueChanged += _ => SetDirty();

        _isInitialized = true;
    }

    public override void RefreshTab()
    {
        _isDirty = false;
    }

    public override void ResetTabToDefault()
    {
        if (_masterVolumeSlider != null) _masterVolumeSlider.SetValueWithoutNotify(1f);
        if (_bgmVolumeSlider != null) _bgmVolumeSlider.SetValueWithoutNotify(1f);
        if (_sfxVolumeSlider != null) _sfxVolumeSlider.SetValueWithoutNotify(1f);
        if (_voiceVolumeSlider != null) _voiceVolumeSlider.SetValueWithoutNotify(1f);
        SetDirty();
    }

    public override void SaveTabSettings()
    {
        _isDirty = false;
    }

    public override bool HasUnsavedChanges()
    {
        return _isDirty;
    }

    private void SetDirty()
    {
        _isDirty = true;
    }
}
