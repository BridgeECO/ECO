using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 타이틀씬 수직 버튼 리스트의 키보드 조작 진입점.
/// 상/하 방향키로 버튼을 순환하며, Enter로 현재 버튼을 클릭한다.
/// 선택 상태는 TitleButtonSelector가 쥐고, 이 클래스는 생명주기와
/// UI_KeyboardInputManager 등록/해제만 담당한다.
/// </summary>
public class UI_TitleButtonsKeyboardHandler : MonoBehaviour, IKeyboardControllable
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<UI_ButtonSelectionItem> _items;

    private TitleScene _titleScene;
    private TitleButtonSelector _selector;
    private bool _isHandlerPushed;

    #region Unity Lifecycle Methods
    // Init은 TitleScene.Start에서 호출되므로, 그보다 앞선 Awake에서 셀렉터를 준비해 둔다.
    private void Awake()
    {
        _selector = new TitleButtonSelector(_items);
    }

    private void OnEnable()
    {
        _selector.InitSelection();
        _selector.BindClickListeners();

        if (_selector.IsSelectionEnabled)
        {
            SafePushHandler();
        }
    }

    private void OnDisable()
    {
        SafePopHandler();
        _selector.UnbindClickListeners();
    }

    private void OnDestroy()
    {
        if (_titleScene != null)
        {
            _titleScene.OnMenuStarted -= HandleMenuStarted;
        }
    }
    #endregion

    public void Init(TitleScene titleScene)
    {
        _titleScene = titleScene;
        if (_titleScene != null)
        {
            _titleScene.OnMenuStarted += HandleMenuStarted;
        }
    }

    private void HandleMenuStarted()
    {
        _selector.SetSelectionEnabled(true);
        SafePushHandler();

        // OnEnable 시점에는 아직 메뉴가 시작되지 않아 선택을 미뤄 두었다. 여기서 처음 반영한다.
        _selector.RefreshSelection();
    }

    #region IKeyboardControllable
    public void OnMoveUp()
    {
        _selector.ChangeSelection(-1);
    }

    public void OnMoveDown()
    {
        _selector.ChangeSelection(1);
    }

    // 타이틀씬은 수직 리스트만 지원하므로 좌/우 입력은 비활성화
    public void OnMoveLeft() { }

    public void OnMoveRight() { }

    public void OnConfirm()
    {
        _selector.InvokeCurrentButton();
    }

    // ESC는 UIManager가 전역 처리하므로 여기서는 응답하지 않음
    public void OnCancel() { }
    #endregion

    #region Handler Registration
    // 매니저 Awake 이전에도 등록되어야 하므로, 캐시만 보는 HasInstance가 아니라
    // 씬 탐색 폴백이 있는 Instance로 확인한다.
    private void SafePushHandler()
    {
        if (_isHandlerPushed || UI_KeyboardInputManager.Instance == null)
        {
            return;
        }

        UI_KeyboardInputManager.Instance.PushHandler(this);
        _isHandlerPushed = true;
    }

    // OnDisable 내부에서 직접 싱글톤 접근을 지양하는 컨벤션 준수를 위해 별도 메서드로 분리
    private void SafePopHandler()
    {
        if (!_isHandlerPushed)
        {
            return;
        }

        _isHandlerPushed = false;
        if (UI_KeyboardInputManager.HasInstance)
        {
            UI_KeyboardInputManager.Instance.PopHandler(this);
        }
    }
    #endregion
}
