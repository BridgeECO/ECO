using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

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

    // 같은 캔버스의 형제 팝업. UIManager 프로퍼티 경유 대신 직접 바인딩한다.
    [SerializeField]
    private UI_Popup_Settings_ResetConfirm _resetConfirmPopup;

    [SerializeField]
    private UI_Popup_Settings_CloseConfirm _closeConfirmPopup;

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
                    SelectTab(index, playSound: true);
                }
            });
        }

        _resetButton.onClick.AddListener(OnClick_Reset);
        _applyButton.onClick.AddListener(OnClick_Apply);
        _backButton.onClick.AddListener(OnClick_Back);
    }

    public override async UniTask OpenAsync()
    {
        int targetIndex = (0 <= _activeTabIndex && _activeTabIndex < _settingTabs.Count) ? _activeTabIndex : 0;

        for (int i = 0; i < _settingTabs.Count; i++)
        {
            _settingTabs[i].InitTab();
        }

        SelectTab(targetIndex, playSound: false);

        await base.OpenAsync();
    }

    public override async UniTask CloseAsync()
    {
        await base.CloseAsync();
    }

    private void SelectTab(int index, bool playSound = true)
    {
        if (index < 0 || index >= _settingTabs.Count)
        {
            return;
        }

        if (playSound)
        {
            SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Setting_ChangeTab);
        }

        _activeTabIndex = index;

        for (int i = 0; i < _settingTabs.Count; i++)
        {
            bool isActive = (i == _activeTabIndex);
            _settingTabs[i].gameObject.SetActive(isActive);
            if (i < _tabToggles.Count && _tabToggles[i] != null)
            {
                _tabToggles[i].SetIsOnWithoutNotify(isActive);
            }
        }

        if (0 <= _activeTabIndex && _activeTabIndex < _settingTabs.Count)
        {
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

    private void HandleReset()
    {
        if (_resetConfirmPopup == null)
        {
            return;
        }

        _resetConfirmPopup.Show();
    }

    private void HandleBack()
    {
        bool hasUnsaved = false;
        if (0 <= _activeTabIndex && _activeTabIndex < _settingTabs.Count)
        {
            hasUnsaved = _settingTabs[_activeTabIndex].HasUnsavedChanges();
        }

        if (hasUnsaved && _closeConfirmPopup != null)
        {
            _closeConfirmPopup.Show();
        }
        else
        {
            Handler?.ClosePopup(this);
        }
    }
}
