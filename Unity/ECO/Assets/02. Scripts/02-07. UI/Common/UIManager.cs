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

    public UI_PopupHandler PopupHandler { get; private set; }

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
        // 최상단 관리자에서 글로벌 UI 키(Esc) 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 팝업이 열려있다면 최상단 팝업 닫기
            if (PopupHandler != null && PopupHandler.HasPopups)
            {
                PopupHandler.CloseLatestPopup();
            }
            // 팝업이 없다면 일시정지 메뉴 열기
            else if (_popupPauseMenu != null)
            {
                PopupHandler.OpenPopup(_popupPauseMenu);
            }
            
            // 기타 다른 인게임 로직 취소를 위해 이벤트 브로드캐스트
            InputHandler.TriggerCancelEvent();
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

