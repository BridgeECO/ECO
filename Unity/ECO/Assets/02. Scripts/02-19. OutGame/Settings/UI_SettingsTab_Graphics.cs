using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_SettingsTab_Graphics : UI_SettingsTabBase
{
    [Foldout("UI")] 
    [SerializeField] private TMP_Dropdown UI_DisplayModeDropdown;
    [SerializeField] private TMP_Dropdown UI_ResolutionDropdown;
    [SerializeField] private Slider UI_BrightnessSlider;
    [SerializeField] private Slider UI_VSyncSwitch; // Slider -> Switch for inspector clarity
    [SerializeField] private TMP_Dropdown UI_OverallQualityDropdown;
    [SerializeField] private Slider UI_TextureQualitySlider;
    [SerializeField] private Slider UI_ShadowQualitySlider;
    [SerializeField] private Slider UI_MotionBlurSlider;
    [SerializeField] private Slider UI_AntiAliasingSlider;

    private bool isDirty;

    public override void Init()
    {
        UI_DisplayModeDropdown.onValueChanged.AddListener(val => SetDirty());
        UI_ResolutionDropdown.onValueChanged.AddListener(val => SetDirty());
        UI_BrightnessSlider.onValueChanged.AddListener(val => SetDirty());
        UI_VSyncSwitch.onValueChanged.AddListener(val => SetDirty());
        UI_OverallQualityDropdown.onValueChanged.AddListener(val => SetDirty());
        UI_TextureQualitySlider.onValueChanged.AddListener(val => SetDirty());
        UI_ShadowQualitySlider.onValueChanged.AddListener(val => SetDirty());
        UI_MotionBlurSlider.onValueChanged.AddListener(val => SetDirty());
        UI_AntiAliasingSlider.onValueChanged.AddListener(val => SetDirty());
    }

    public override void Refresh()
    {
        isDirty = false;
    }

    public override void ResetToDefault()
    {
        UI_BrightnessSlider.value = 1f;
        UI_VSyncSwitch.value = 1f;
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
