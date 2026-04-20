using Cysharp.Threading.Tasks;
using Ricimi;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_SettingsPopup : Popup
{
    [Foldout("UI")]
    [SerializeField] private List<Toggle> UI_TabToggles; 
    [SerializeField] private List<UI_SettingsTabBase> UI_SettingTabs;
    [SerializeField] private Button UI_ResetButton;
    [SerializeField] private Button UI_ApplyButton;
    [SerializeField] private Button UI_BackButton;
    [SerializeField] private Image UI_BackgroundOverlay;
    
    [Foldout("References")]
    [SerializeField] private UI_SystemPopup UI_ConfirmPopup;

    private int activeTabIndex = 0;

    private void Awake()
    {
        for (int i = 0; i < UI_TabToggles.Count; i++)
        {
            int index = i;
            UI_TabToggles[i].onValueChanged.AddListener((isOn) => 
            {
                if (isOn)
                {
                    OnClick_Tab(index);
                }
            });
        }

        UI_ResetButton.onClick.AddListener(OnClick_Reset);
        UI_ApplyButton.onClick.AddListener(OnClick_Apply);
        UI_BackButton.onClick.AddListener(OnClick_Back);
    }
    
    private void OnClick_Tab(int index)
    {
        if (activeTabIndex >= 0 && activeTabIndex < UI_SettingTabs.Count)
        {
            UI_SettingTabs[activeTabIndex].gameObject.SetActive(false);
        }

        activeTabIndex = index;
        
        if (activeTabIndex >= 0 && activeTabIndex < UI_SettingTabs.Count)
        {
            UI_SettingTabs[activeTabIndex].gameObject.SetActive(true);
            UI_SettingTabs[activeTabIndex].Refresh();
        }
    }

    private void OnClick_Reset()
    {
        HandleResetAsync().Forget();
    }

    private void OnClick_Apply()
    {
        if (activeTabIndex >= 0 && activeTabIndex < UI_SettingTabs.Count)
        {
            UI_SettingTabs[activeTabIndex].SaveSettings();
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

        for (int i = 0; i < UI_SettingTabs.Count; i++)
        {
            UI_SettingTabs[i].Init();
            UI_SettingTabs[i].gameObject.SetActive(false);
        }

        if (UI_TabToggles.Count > 0)
        {
            UI_TabToggles[0].isOn = true;
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

        if (UI_BackgroundOverlay != null)
        {
            UI_BackgroundOverlay.CrossFadeAlpha(0.0f, 0.2f, false);
        }

        WaitAndDisableAsync().Forget();
    }

    private void CustomOpen()
    {
        if (UI_BackgroundOverlay != null)
        {
            UI_BackgroundOverlay.gameObject.SetActive(true);
            UI_BackgroundOverlay.canvasRenderer.SetAlpha(0.0f);
            UI_BackgroundOverlay.CrossFadeAlpha(1.0f, 0.4f, false);
        }
    }

    private async UniTaskVoid HandleResetAsync()
    {
        UI_SystemPopup.EPopupResult result = await UI_ConfirmPopup.ShowPopupAsync(
            "초기화", 
            "모든 설정을 기본값으로 되돌리시겠습니까?", 
            false
        );

        if (result == UI_SystemPopup.EPopupResult.Confirm)
        {
            if (activeTabIndex >= 0 && activeTabIndex < UI_SettingTabs.Count)
            {
                UI_SettingTabs[activeTabIndex].ResetToDefault();
            }
        }
    }

    private async UniTaskVoid HandleBackAsync()
    {
        bool hasUnsaved = false;
        if (activeTabIndex >= 0 && activeTabIndex < UI_SettingTabs.Count)
        {
            hasUnsaved = UI_SettingTabs[activeTabIndex].HasUnsavedChanges();
        }

        if (hasUnsaved)
        {
            UI_SystemPopup.EPopupResult result = await UI_ConfirmPopup.ShowPopupAsync(
                "저장 경고", 
                "저장하지 않고 나가시겠습니까?", 
                true
            );

            if (result == UI_SystemPopup.EPopupResult.Confirm)
            {
                UI_SettingTabs[activeTabIndex].SaveSettings();
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
