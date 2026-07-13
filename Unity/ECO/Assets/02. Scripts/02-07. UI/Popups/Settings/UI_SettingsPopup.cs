using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Ricimi;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_SettingsPopup : UI_Popup
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<Toggle> _tabToggles;

    [SerializeField]
    private List<UI_SettingsTabBase> _settingTabs;

    [SerializeField]
    private Button _resetButton;

    [SerializeField]
    private Button _applyButton;

    [SerializeField]
    private Button _backButton;

    [SerializeField]
    private Image _backgroundOverlay;


    private int _activeTabIndex = 0;

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
        CustomOpen();

        for (int i = 0; i < _settingTabs.Count; i++)
        {
            _settingTabs[i].InitTab();
            _settingTabs[i].gameObject.SetActive(false);
        }

        if (0< _tabToggles.Count )
        {
            _tabToggles[0].isOn = true;
            OnClick_Tab(0);
        }
    }

    public override async UniTask CloseAsync()
    {
        if (_backgroundOverlay != null)
        {
            _backgroundOverlay.CrossFadeAlpha(0.0f, 0.2f, true);
        }

        await base.CloseAsync();
    }

    private void CustomOpen()
    {
        if (_backgroundOverlay != null)
        {
            _backgroundOverlay.gameObject.SetActive(true);
            _backgroundOverlay.canvasRenderer.SetAlpha(0.0f);
            _backgroundOverlay.CrossFadeAlpha(1.0f, 0.4f, true);
        }
    }

    private void HandleReset()
    {
        if (UIManager.Instance.SettingsResetConfirmPopup == null)
        {
            return;
        }

        UIManager.Instance.SettingsResetConfirmPopup.Show(
            "설정 초기화",
            "기본 설정값으로 초기화하시겠습니까?",
            onConfirm: () =>
            {
                if (0 <= _activeTabIndex && _activeTabIndex < _settingTabs.Count)
                {
                    _settingTabs[_activeTabIndex].ResetTabToDefault();
                }
            }
        );
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
            UIManager.Instance.SettingsCloseConfirmPopup.Show(
                "변경사항 미저장",
                "저장하지 않고 나가시겠습니까?",
                onAction1: () =>
                {
                    if (0 <= _activeTabIndex && _activeTabIndex < _settingTabs.Count)
                    {
                        _settingTabs[_activeTabIndex].SaveTabSettings();
                    }
                    UIManager.Instance.PopupHandler.ClosePopup(this);
                },
                onAction2: () =>
                {
                    UIManager.Instance.PopupHandler.ClosePopup(this);
                }
            );
        }
        else
        {
            UIManager.Instance.PopupHandler.ClosePopup(this);
        }
    }


}
