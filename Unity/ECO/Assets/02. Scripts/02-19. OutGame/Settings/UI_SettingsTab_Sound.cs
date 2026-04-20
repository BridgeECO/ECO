using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_SettingsTab_Sound : UI_SettingsTabBase
{
    [Foldout("UI")]
    [SerializeField] private Slider UI_MasterVolumeSlider;
    [SerializeField] private Slider UI_BGMVolumeSlider;
    [SerializeField] private Slider UI_SFXVolumeSlider;
    [SerializeField] private Slider UI_VoiceVolumeSlider;

    private bool isDirty;

    public override void Init()
    {
        UI_MasterVolumeSlider.onValueChanged.AddListener(val => SetDirty());
        UI_BGMVolumeSlider.onValueChanged.AddListener(val => SetDirty());
        UI_SFXVolumeSlider.onValueChanged.AddListener(val => SetDirty());
        UI_VoiceVolumeSlider.onValueChanged.AddListener(val => SetDirty());
    }

    public override void Refresh()
    {
        isDirty = false;
    }

    public override void ResetToDefault()
    {
        UI_MasterVolumeSlider.value = 1f;
        UI_BGMVolumeSlider.value = 1f;
        UI_SFXVolumeSlider.value = 1f;
        UI_VoiceVolumeSlider.value = 1f;
        SetDirty();
    }

    public override void SaveSettings()
    {
        isDirty = false;
    }

    public override bool HasUnsavedChanges()
    {
        return isDirty;
    }

    private void SetDirty()
    {
        isDirty = true;
    }
}
