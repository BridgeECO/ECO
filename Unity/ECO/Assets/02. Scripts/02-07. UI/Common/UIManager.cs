using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _loadingPanel;

    [SerializeField]
    private UI_PauseMenuPopup _popupPauseMenu;

    [SerializeField]
    private UI_SettingsPopup _popupSettings;

    [SerializeField]
    private UI_Popup_Settings_CloseConfirm _popupSettingsCloseConfirm;

    [SerializeField]
    private UI_Popup_Settings_ResetConfirm _popupSettingsResetConfirm;

    [SerializeField]
    private UI_Popup_ExitConfirm _popupExitConfirm;

    public UI_PopupHandler PopupHandler { get; private set; }
    public UI_Popup_Settings_CloseConfirm SettingsCloseConfirmPopup => _popupSettingsCloseConfirm;
    public UI_Popup_Settings_ResetConfirm SettingsResetConfirmPopup => _popupSettingsResetConfirm;
    public UI_Popup_ExitConfirm ExitConfirmPopup => _popupExitConfirm;

    protected override void Awake()
    {
        base.Awake();
        PopupHandler = new UI_PopupHandler();
    }

    private void Start()
    {
        PopupHandler.Init();
    }

    private void OnDisable()
    {
        PopupHandler.Dispose();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeInput();
        }
    }

    private void HandleEscapeInput()
    {
        // 팝업이 열려있다면 최상단 팝업 닫기
        if (PopupHandler != null && PopupHandler.HasPopups)
        {
            PopupHandler.CloseLatestPopup();
        }
        // 팝업이 없다면 일시정지 메뉴 열기 (인게임 플레이 중일 때만 허용)
        else if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsGameplayScene)
        {
            OpenPauseMenuPopup();
        }

        InputHandler.TriggerCancelEvent();
    }


    public void OpenPauseMenuPopup()
    {
        if (_popupPauseMenu != null)
        {
            PopupHandler.OpenPopup(_popupPauseMenu);
        }
    }

    public void OpenSettingsPopup()
    {
        if (_popupSettings != null)
        {
            PopupHandler.OpenPopup(_popupSettings);
        }
    }

    public void FadeInLoadingPanel(Action onComplete = null)
    {
        _loadingPanel.DOFade(1f, 1f).SetEase(Ease.InQuad)
        .OnComplete(() => onComplete?.Invoke());
    }

    public void FadeOutLoadingPanel(Action onComplete = null)
    {
        _loadingPanel.DOFade(0f, 1f).SetEase(Ease.OutQuad)
        .OnComplete(() => onComplete?.Invoke());
    }
}

