using UnityEngine;

/// <summary>
/// 키보드 입력을 자체 처리하지 않는 UI 패널/팝업 상에 부착되어,
/// 배경 UI(예: 메인 메뉴)로 키보드 방향키 및 확인/취소 입력이 무단 전달되는 것을 차단하는 컴포넌트.
/// OnEnable/OnDisable 시점에 UI_KeyboardInputManager에 등록/해제된다.
/// </summary>
public class UI_KeyboardInputBlocker : MonoBehaviour, IKeyboardControllable
{
    #region Unity Lifecycle Methods
    private void OnEnable()
    {
        UI_KeyboardInputManager.Instance.PushHandler(this);
    }

    private void OnDisable()
    {
        SafePopHandler();
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

    #region Cleanup
    private void SafePopHandler()
    {
        if (UI_KeyboardInputManager.Instance != null)
        {
            UI_KeyboardInputManager.Instance.PopHandler(this);
        }
    }
    #endregion
}
