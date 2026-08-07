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

    // 씬 전환 상태를 매 프레임 싱글턴 조회하는 대신 이벤트로 통지받아 캐싱한다.
    private bool _isTransitioning;
    private bool _isGameplayScene;
    private bool _isListenerAdded;

    public UI_PopupHandler PopupHandler { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        PopupHandler = new UI_PopupHandler();
        InitPopupHandlers();
    }

    // 등록된 팝업들에 핸들러를 주입한다. 팝업이 스스로를 열어야 하는 경우(Show 계열)에도
    // UIManager 싱글턴을 역참조하지 않게 하기 위함이다.
    private void InitPopupHandlers()
    {
        UI_Popup[] popups =
        {
            _popupPauseMenu, _popupSettings, _popupSettingsCloseConfirm,
            _popupSettingsResetConfirm, _popupExitConfirm, _popupNewGameConfirm
        };
        foreach (UI_Popup popup in popups)
        {
            if (popup != null)
            {
                popup.SetHandler(PopupHandler);
            }
        }
    }

    private void OnEnable()
    {
        AddEventListeners();
    }

    private void Start()
    {
        PopupHandler.Init();

        // OnEnable 시점에는 EventManager의 Awake가 아직 안 돌았을 수 있어 한 번 더 시도한다.
        AddEventListeners();

        // 구독 이전에 확정된 상태(에디터 멀티 씬 채택 등)를 한 번 동기화한다.
        if (SceneTransitionManager.Instance != null)
        {
            _isTransitioning = SceneTransitionManager.Instance.IsTransitioning;
            _isGameplayScene = SceneTransitionManager.Instance.IsGameplayScene;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !_isTransitioning)
        {
            HandleEscapeInput();
        }
    }

    private void OnDisable()
    {
        PopupHandler.Dispose();
        RemoveEventListeners();
    }

    // OnEnable과 Start 양쪽에서 호출되므로 중복 등록을 플래그로 막는다.
    private void AddEventListeners()
    {
        if (_isListenerAdded || EventManager.Instance == null)
        {
            return;
        }

        EventManager.Instance.AddEventListener<bool>(EEventType.TransitionStateChanged, SetTransitioning);
        EventManager.Instance.AddEventListener<bool>(EEventType.GameplaySceneChanged, SetGameplayScene);
        _isListenerAdded = true;
    }

    private void RemoveEventListeners()
    {
        if (!_isListenerAdded)
        {
            return;
        }

        _isListenerAdded = false;
        if (EventManager.HasInstance)
        {
            EventManager.Instance.RemoveEventListener<bool>(EEventType.TransitionStateChanged, SetTransitioning);
            EventManager.Instance.RemoveEventListener<bool>(EEventType.GameplaySceneChanged, SetGameplayScene);
        }
    }

    private void SetTransitioning(bool isTransitioning)
    {
        _isTransitioning = isTransitioning;
    }

    private void SetGameplayScene(bool isGameplayScene)
    {
        _isGameplayScene = isGameplayScene;
    }

    private void HandleEscapeInput()
    {
        // 팝업이 열려있다면 최상단 팝업 닫기
        if (PopupHandler != null && PopupHandler.HasPopups)
        {
            PopupHandler.CloseLatestPopup();
        }
        // 팝업이 없다면 일시정지 메뉴 열기 (인게임 플레이 중일 때만 허용)
        else if (_isGameplayScene)
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

    // 타이틀 씬 등 다른 씬의 UI가 PersistentScene의 팝업을 직접 참조할 수 없으므로
    // 크로스 씬 진입점은 의도가 드러나는 메서드로만 노출한다.
    public void OpenExitConfirmPopup()
    {
        if (_popupExitConfirm != null)
        {
            _popupExitConfirm.Show();
        }
    }

    /// <summary>새 게임 확인 팝업을 띄우고 플레이어가 확인을 눌렀는지 반환한다.</summary>
    public async UniTask<bool> ShowNewGameConfirmAsync(System.Threading.CancellationToken cancellationToken)
    {
        if (_popupNewGameConfirm == null)
        {
            return true;
        }
        UI_Popup_NewGameConfirm.EPopupResult result = await _popupNewGameConfirm.ShowPopupAsync(cancellationToken);
        return result == UI_Popup_NewGameConfirm.EPopupResult.Confirm;
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