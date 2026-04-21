using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_SettingsTab_Gameplay : UI_SettingsTabBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Dropdown _languageDropdown;

    [SerializeField]
    private Slider _subtitlesSwitch;

    [SerializeField]
    private Slider _cameraShakeSwitch;

    [SerializeField]
    private Slider _uiScaleSlider;

    private bool _isDirty;

    public override void InitTab()
    {
        _languageDropdown.onValueChanged.AddListener(val => SetDirty());
        _subtitlesSwitch.onValueChanged.AddListener(val => SetDirty());
        _cameraShakeSwitch.onValueChanged.AddListener(val => SetDirty());
        _uiScaleSlider.onValueChanged.AddListener(val => SetDirty());
    }

    public override void RefreshTab()
    {
        _isDirty = false;
    }

    public override void ResetTabToDefault()
    {
        _subtitlesSwitch.value = 1f;
        _cameraShakeSwitch.value = 1f;
        _uiScaleSlider.value = 1f;
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
