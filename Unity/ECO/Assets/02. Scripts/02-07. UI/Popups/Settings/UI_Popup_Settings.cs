using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Ricimi;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

// Force compilation trigger
public class UI_Popup_Settings : UI_Popup
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<Toggle> _tabToggles = new();

    [SerializeField]
    private List<UI_SettingsTabBase> _settingTabs = new();

    [SerializeField]
    private Button _resetButton;

    [SerializeField]
    private Button _applyButton;

    [SerializeField]
    private Button _backButton;

    [SerializeField]
    private UI_TextFadeFeedback _settingsFeedback;

    private int _activeTabIndex = 0;

    public UI_SettingsTabBase ActiveTab => (0 <= _activeTabIndex && _activeTabIndex < _settingTabs.Count) ? _settingTabs[_activeTabIndex] : null;
    public UI_TextFadeFeedback SettingsFeedback => _settingsFeedback;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < _tabToggles.Count; i++)
        {
            int index = i;
            _tabToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    OnClick_Tab(index);
                }
            });
        }
        _resetButton.onClick.AddListener(OnClick_Reset);
        _applyButton.onClick.AddListener(OnClick_Apply);
        _backButton.onClick.AddListener(OnClick_Back);
    }

    private void OnClick_Tab(int index)
    {
        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Setting_ChangeTab);
        if (0 <= _activeTabIndex && _activeTabIndex < _settingTabs.Count)
        {
            _settingTabs[_activeTabIndex].gameObject.SetActive(false);
        }
        _activeTabIndex = index;
        if (0 <= _activeTabIndex && _activeTabIndex < _settingTabs.Count)
        {
            _settingTabs[_activeTabIndex].gameObject.SetActive(true);
            _settingTabs[_activeTabIndex].RefreshTab();
        }
    }

    private void OnClick_Reset()
    {
        if (IsClosing)
        {
            return;
        }
        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Common_Button);
        HandleReset();
    }

    private void OnClick_Apply()
    {
        if (IsClosing)
        {
            return;
        }
        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Common_Button);
        if (0 <= _activeTabIndex && _activeTabIndex < _settingTabs.Count)
        {
            _settingTabs[_activeTabIndex].SaveTabSettings();
            if (_settingsFeedback != null)
            {
                _settingsFeedback.Play("변경사항이 적용되었습니다.");
            }
        }
    }

    private void OnClick_Back()
    {
        if (IsClosing)
        {
            return;
        }
        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Common_Button);
        HandleBack();
    }

    public override async UniTask OpenAsync()
    {
        await base.OpenAsync();
        for (int i = 0; i < _settingTabs.Count; i++)
        {
            _settingTabs[i].InitTab();
            _settingTabs[i].gameObject.SetActive(false);
        }
        if (0 < _tabToggles.Count)
        {
            _tabToggles[0].isOn = true;
            OnClick_Tab(0);
        }
    }

    public override async UniTask CloseAsync()
    {
        await base.CloseAsync();
    }

    private void HandleReset()
    {
        if (UIManager.Instance.SettingsResetConfirmPopup == null)
        {
            return;
        }
        UIManager.Instance.SettingsResetConfirmPopup.Show();
    }

    private void HandleBack()
    {
        bool hasUnsaved = false;
        if (0 <= _activeTabIndex && _activeTabIndex < _settingTabs.Count)
        {
            hasUnsaved = _settingTabs[_activeTabIndex].HasUnsavedChanges();
        }
        if (hasUnsaved && UIManager.Instance.SettingsCloseConfirmPopup != null)
        {
            UIManager.Instance.SettingsCloseConfirmPopup.Show();
        }
        else
        {
            UIManager.Instance.PopupHandler.ClosePopup(this);
        }
    }
}
