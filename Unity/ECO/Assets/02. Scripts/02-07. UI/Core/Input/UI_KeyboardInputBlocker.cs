using UnityEngine;

/// <summary>
/// 키보드 입력을 자체 처리하지 않는 UI 패널/팝업 상에 부착되어,
/// 배경 UI(예: 메인 메뉴)로 키보드 방향키 및 확인/취소 입력이 무단 전달되는 것을 차단하는 컴포넌트.
/// OnEnable/OnDisable 시점에 UI_KeyboardInputManager에 등록/해제된다.
/// </summary>
public class UI_KeyboardInputBlocker : MonoBehaviour, IKeyboardControllable
{
    private readonly KeyboardHandlerRegistration _registration = new KeyboardHandlerRegistration();

    #region Unity Lifecycle Methods
    private void OnEnable()
    {
        _registration.Push(this);
    }

    // 이 팝업이 속한 씬이 매니저가 있는 PersistentScene보다 먼저 로드되면
    // OnEnable 시점에는 매니저가 아직 없다. 모든 Awake가 끝난 Start에서 한 번 더 시도한다.
    private void Start()
    {
        _registration.Push(this);
    }

    private void OnDisable()
    {
        _registration.Pop(this);
    }
    #endregion

    #region IKeyboardControllable
    public void OnMoveUp()
    {
    }

    public void OnMoveDown()
    {
    }

    public void OnMoveLeft()
    {
    }

    public void OnMoveRight()
    {
    }

    public void OnConfirm()
    {
    }

    public void OnCancel()
    {
    }
    #endregion
}
