using UnityEngine;
using VInspector;

public class UI_SettingsTab_Gameplay : UI_SettingsTabBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_Spinner _languageSpinner;

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

        if (_languageSpinner != null) _languageSpinner.OnValueChanged += (index, option) => SetDirty();

        _isInitialized = true;
    }

    public override void RefreshTab()
    {
        _isDirty = false;
    }

    public override void ResetTabToDefault()
    {
        if (_languageSpinner != null) _languageSpinner.SetValueWithoutNotify(0);
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
