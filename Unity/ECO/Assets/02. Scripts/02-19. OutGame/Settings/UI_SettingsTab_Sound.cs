using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_SettingsTab_Sound : UI_SettingsTabBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Slider _masterVolumeSlider;

    [SerializeField]
    private Slider _bgmVolumeSlider;

    [SerializeField]
    private Slider _sfxVolumeSlider;

    [SerializeField]
    private Slider _voiceVolumeSlider;

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

        _masterVolumeSlider.onValueChanged.AddListener(val => SetDirty());
        _bgmVolumeSlider.onValueChanged.AddListener(val => SetDirty());
        _sfxVolumeSlider.onValueChanged.AddListener(val => SetDirty());
        _voiceVolumeSlider.onValueChanged.AddListener(val => SetDirty());

        _isInitialized = true;
    }

    public override void RefreshTab()
    {
        _isDirty = false;
    }

    public override void ResetTabToDefault()
    {
        _masterVolumeSlider.value = 1f;
        _bgmVolumeSlider.value = 1f;
        _sfxVolumeSlider.value = 1f;
        _voiceVolumeSlider.value = 1f;
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
