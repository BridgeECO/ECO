using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Ricimi;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_SettingsPopup : Popup
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


    [Foldout("Hierarchy")]
    [Header("References")]
    [SerializeField]
    private UI_SystemPopup _confirmPopup;

    private int _activeTabIndex = 0;

    private void Awake()
    {
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
        if (_activeTabIndex >= 0 && _activeTabIndex < _settingTabs.Count)
        {
            _settingTabs[_activeTabIndex].gameObject.SetActive(false);
        }

        _activeTabIndex = index;


        if (_activeTabIndex >= 0 && _activeTabIndex < _settingTabs.Count)
        {
            _settingTabs[_activeTabIndex].gameObject.SetActive(true);
            _settingTabs[_activeTabIndex].RefreshTab();
        }
    }

    private void OnClick_Reset()
    {
        HandleResetAsync().Forget();
    }

    private void OnClick_Apply()
    {
        if (_activeTabIndex >= 0 && _activeTabIndex < _settingTabs.Count)
        {
            _settingTabs[_activeTabIndex].SaveTabSettings();
        }
    }

    private void OnClick_Back()
    {
        HandleBackAsync().Forget();
    }

    public void ShowSettings()
    {
        gameObject.SetActive(true);
        CustomOpen();

        for (int i = 0; i < _settingTabs.Count; i++)
        {
            _settingTabs[i].InitTab();
            _settingTabs[i].gameObject.SetActive(false);
        }

        if (_tabToggles.Count > 0)
        {
            _tabToggles[0].isOn = true;
            OnClick_Tab(0);
        }
    }

    public new void Close()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
        {
            animator.Play("Close");
        }

        if (_backgroundOverlay != null)
        {
            _backgroundOverlay.CrossFadeAlpha(0.0f, 0.2f, false);
        }

        WaitAndDisableAsync().Forget();
    }

    private void CustomOpen()
    {
        if (_backgroundOverlay != null)
        {
            _backgroundOverlay.gameObject.SetActive(true);
            _backgroundOverlay.canvasRenderer.SetAlpha(0.0f);
            _backgroundOverlay.CrossFadeAlpha(1.0f, 0.4f, false);
        }
    }

    private async UniTaskVoid HandleResetAsync()
    {
        UI_SystemPopup.EPopupResult result = await _confirmPopup.ShowPopupAsync(
            "초기화",
            "모든 설정을 기본값으로 되돌리시겠습니까?",
            false
        );

        if (result == UI_SystemPopup.EPopupResult.Confirm)
        {
            if (_activeTabIndex >= 0 && _activeTabIndex < _settingTabs.Count)
            {
                _settingTabs[_activeTabIndex].ResetTabToDefault();
            }
        }
    }

    private async UniTaskVoid HandleBackAsync()
    {
        bool hasUnsaved = false;
        if (_activeTabIndex >= 0 && _activeTabIndex < _settingTabs.Count)
        {
            hasUnsaved = _settingTabs[_activeTabIndex].HasUnsavedChanges();
        }

        if (hasUnsaved)
        {
            UI_SystemPopup.EPopupResult result = await _confirmPopup.ShowPopupAsync(
                "Unsaved Changes",
                "You have unsaved changes. Would you like to confirm the changes before leaving, or ignore them and exit anyway?",
                true
            );

            if (result == UI_SystemPopup.EPopupResult.Confirm)
            {
                _settingTabs[_activeTabIndex].SaveTabSettings();
                Close();
            }
            else if (result == UI_SystemPopup.EPopupResult.Ignore)
            {
                Close();
            }
        }
        else
        {
            Close();
        }
    }

    private async UniTaskVoid WaitAndDisableAsync()
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(destroyTime));
        gameObject.SetActive(false);
    }
}
