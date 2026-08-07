using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _fadeInOutPanel;

    [SerializeField]
    private UI_Popup_PauseMenu _popupPauseMenu;

    [SerializeField]
    private UI_Popup_Settings _popupSettings;

    [SerializeField]
    private UI_Popup_Settings_CloseConfirm _popupSettingsCloseConfirm;

    [SerializeField]
    private UI_Popup_Settings_ResetConfirm _popupSettingsResetConfirm;

    [SerializeField]
    private UI_Popup_ExitConfirm _popupExitConfirm;

    [SerializeField]
    private UI_Popup_NewGameConfirm _popupNewGameConfirm;

    public UI_PopupHandler PopupHandler { get; private set; }
    public UI_Popup_Settings SettingsPopup => _popupSettings;
    public IUIConfirm3Buttons SettingsCloseConfirmPopup => _popupSettingsCloseConfirm;
    public IUIConfirm2Buttons SettingsResetConfirmPopup => _popupSettingsResetConfirm;
    public IUIConfirm2Buttons ExitConfirmPopup => _popupExitConfirm;
    public UI_Popup_NewGameConfirm NewGameConfirmPopup => _popupNewGameConfirm;

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
        if (Input.GetKeyDown(KeyCode.Escape) && !(SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning))
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

    public void FadeIn(float duration = 1f, Action onComplete = null)
    {
        if (_fadeInOutPanel == null)
        {
            onComplete?.Invoke();
            return;
        }
        _fadeInOutPanel.DOFade(0f, duration).SetEase(Ease.OutQuad).SetUpdate(true)
        .OnComplete(() => 
        {
            _fadeInOutPanel.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    public void FadeOut(float duration = 1f, Action onComplete = null)
    {
        if (_fadeInOutPanel == null)
        {
            onComplete?.Invoke();
            return;
        }
        _fadeInOutPanel.gameObject.SetActive(true);
        Color color = _fadeInOutPanel.color;
        color.a = 0f;
        _fadeInOutPanel.color = color;

        _fadeInOutPanel.DOFade(1f, duration).SetEase(Ease.InQuad).SetUpdate(true)
        .OnComplete(() => 
        { 
            onComplete?.Invoke(); 
        });
    }

    public async UniTask FadeInAsync(float duration = 1f, System.Threading.CancellationToken cancellationToken = default)
    {
        if (_fadeInOutPanel == null)
        {
            return;
        }
        await _fadeInOutPanel.DOFade(0f, duration).SetEase(Ease.OutQuad).SetUpdate(true)
            .ToUniTask(cancellationToken: cancellationToken);
        _fadeInOutPanel.gameObject.SetActive(false);
    }

    public async UniTask FadeOutAsync(float duration = 1f, System.Threading.CancellationToken cancellationToken = default)
    {
        if (_fadeInOutPanel == null)
        {
            return;
        }
        _fadeInOutPanel.gameObject.SetActive(true);
        Color color = _fadeInOutPanel.color;
        color.a = 0f;
        _fadeInOutPanel.color = color;

        await _fadeInOutPanel.DOFade(1f, duration).SetEase(Ease.InQuad).SetUpdate(true)
            .ToUniTask(cancellationToken: cancellationToken);
    }


}