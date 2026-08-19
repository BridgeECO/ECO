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
    private readonly KeyboardHandlerRegistration _registration = new KeyboardHandlerRegistration();

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
            _registration.Push(this);
        }
    }

    private void OnDisable()
    {
        _registration.Pop(this);
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

        // 구독은 OnDestroy까지 살아 있어 비활성 상태에서도 이 콜백이 올 수 있다. 그때 등록하면
        // 화면에 없는 타이틀 버튼이 키 입력을 받는다. 메뉴 시작 여부는 위에서 셀렉터가 이미 기억했으므로
        // 다음 OnEnable에서 정상적으로 등록된다.
        if (!isActiveAndEnabled)
        {
            return;
        }

        _registration.Push(this);

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
}
