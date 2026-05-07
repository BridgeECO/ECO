using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_SettingsTab_Graphics : UI_SettingsTabBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Dropdown _displayModeDropdown;

    [SerializeField]
    private TMP_Dropdown _resolutionDropdown;

    [SerializeField]
    private Slider _brightnessSlider;

    [SerializeField]
    private Slider _vSyncSwitch;

    [SerializeField]
    private TMP_Dropdown _overallQualityDropdown;

    [SerializeField]
    private Slider _textureQualitySlider;

    [SerializeField]
    private Slider _shadowQualitySlider;

    [SerializeField]
    private Slider _motionBlurSlider;

    [SerializeField]
    private Slider _antiAliasingSlider;

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

        _displayModeDropdown.onValueChanged.AddListener(val => SetDirty());
        _resolutionDropdown.onValueChanged.AddListener(val => SetDirty());
        _brightnessSlider.onValueChanged.AddListener(val => SetDirty());
        _vSyncSwitch.onValueChanged.AddListener(val => SetDirty());
        _overallQualityDropdown.onValueChanged.AddListener(val => SetDirty());
        _textureQualitySlider.onValueChanged.AddListener(val => SetDirty());
        _shadowQualitySlider.onValueChanged.AddListener(val => SetDirty());
        _motionBlurSlider.onValueChanged.AddListener(val => SetDirty());
        _antiAliasingSlider.onValueChanged.AddListener(val => SetDirty());

        _isInitialized = true;
    }

    public override void RefreshTab()
    {
        _isDirty = false;
    }

    public override void ResetTabToDefault()
    {
        _brightnessSlider.value = 1f;
        _vSyncSwitch.value = 1f;
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
