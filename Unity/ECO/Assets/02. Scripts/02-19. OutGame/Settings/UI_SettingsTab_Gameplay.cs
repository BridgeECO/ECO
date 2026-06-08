using UnityEngine;
using VInspector;

public class UI_SettingsTab_Gameplay : UI_SettingsTabBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_Spinner _languageSpinner;

    private bool _isDirty;
    private bool _isInitialized;

    private int _currentLanguageIndex;

    private const string PrefKey_Language = "Settings_Gameplay_Language";

    private void Awake()
    {
        InitTab();
    }

    private void OnDisable()
    {
        if (_isDirty)
        {
            LoadSavedSettings();
            SyncUIToCurrentValues();
            _isDirty = false;
        }
    }

    public override void InitTab()
    {
        if (_isInitialized)
        {
            return;
        }

        LoadSavedSettings();
        SyncUIToCurrentValues();

        if (_languageSpinner != null) _languageSpinner.OnValueChanged += (index, option) => { _currentLanguageIndex = index; ApplySettingsImmediately(); SetDirty(); };

        _isInitialized = true;
    }

    private void LoadSavedSettings()
    {
        _currentLanguageIndex = PlayerPrefs.GetInt(PrefKey_Language, 0);
    }

    private void SyncUIToCurrentValues()
    {
        if (_languageSpinner != null) _languageSpinner.SetValueWithoutNotify(_currentLanguageIndex);
    }

    private void ApplySettingsImmediately()
    {
        // 실제 게임플레이 매니저(언어 설정)와 연동하는 코드 작성 필요
    }

    public override void RefreshTab()
    {
        SyncUIToCurrentValues();
    }

    public override void ResetTabToDefault()
    {
        _currentLanguageIndex = 0;

        SyncUIToCurrentValues();
        ApplySettingsImmediately();
        SetDirty();
    }

    public override void SaveTabSettings()
    {
        PlayerPrefs.SetInt(PrefKey_Language, _currentLanguageIndex);
        PlayerPrefs.Save();

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
