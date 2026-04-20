using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_SettingsTab_Gameplay : UI_SettingsTabBase
{
    [Foldout("UI")]
    [SerializeField] private TMP_Dropdown UI_LanguageDropdown;
    [SerializeField] private Slider UI_SubtitlesSwitch; // Slider -> Switch for inspector clarity
    [SerializeField] private Slider UI_CameraShakeSwitch; // Slider -> Switch for inspector clarity
    [SerializeField] private Slider UI_UIScaleSlider;

    private bool isDirty;

    public override void Init()
    {
        UI_LanguageDropdown.onValueChanged.AddListener(val => SetDirty());
        UI_SubtitlesSwitch.onValueChanged.AddListener(val => SetDirty());
        UI_CameraShakeSwitch.onValueChanged.AddListener(val => SetDirty());
        UI_UIScaleSlider.onValueChanged.AddListener(val => SetDirty());
    }

    public override void Refresh()
    {
        isDirty = false;
    }

    public override void ResetToDefault()
    {
        UI_SubtitlesSwitch.value = 1f;
        UI_CameraShakeSwitch.value = 1f;
        UI_UIScaleSlider.value = 1f;
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
