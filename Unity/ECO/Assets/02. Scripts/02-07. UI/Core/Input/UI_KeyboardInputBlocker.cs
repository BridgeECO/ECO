using UnityEngine;

/// <summary>
/// 키보드 입력을 자체 처리하지 않는 UI 패널/팝업 상에 부착되어,
/// 배경 UI(예: 메인 메뉴)로 키보드 방향키 및 확인/취소 입력이 무단 전달되는 것을 차단하는 컴포넌트.
/// OnEnable/OnDisable 시점에 UI_KeyboardInputManager에 등록/해제된다.
/// </summary>
public class UI_KeyboardInputBlocker : MonoBehaviour, IKeyboardControllable
{
    private bool _isHandlerPushed;

    #region Unity Lifecycle Methods
    private void OnEnable()
    {
        SafePushHandler();
    }

    // 이 팝업이 속한 씬이 매니저가 있는 PersistentScene보다 먼저 로드되면
    // OnEnable 시점에는 매니저가 아직 없다. 모든 Awake가 끝난 Start에서 한 번 더 시도한다.
    private void Start()
    {
        SafePushHandler();
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

    #region Handler Registration
    // OnEnable과 Start 양쪽에서 호출되므로 중복 등록을 플래그로 막는다.
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

    // 등록에 성공했다면 캐시가 이미 채워져 있으므로, 해제 시점의 씬 탐색을 피해 HasInstance로 확인한다.
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
