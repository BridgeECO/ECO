/// <summary>
/// IKeyboardControllable 구현체가 UI_KeyboardInputManager에 자신을 등록/해제하는 절차.
/// 등록 여부를 자체 플래그로 기억하므로 중복 등록과 미등록 해제가 모두 안전하다.
/// 등록부와 해제부가 서로 다른 싱글톤 조회 API를 쓰는 이유를 이 한 곳에 모아 둔다.
///
/// 비활성 상태로 씬에 놓인 컴포넌트는 Awake가 아직 돌지 않았으므로, 소유 클래스는
/// 이 객체를 Awake가 아니라 필드 이니셜라이저에서 생성해야 한다. 그래서 핸들러를
/// 생성자로 받지 않고 호출 시점에 넘겨받는다.
/// </summary>
public class KeyboardHandlerRegistration
{
    private bool _isPushed;

    /// <summary>
    /// 매니저의 Awake보다 먼저 도는 OnEnable에서도 등록되어야 하므로, 캐시만 확인하는
    /// HasInstance가 아니라 씬 탐색 폴백이 있는 Instance로 조회한다.
    /// 매니저가 없는 씬(자립형 테스트 씬 등)에서는 조용히 넘어간다.
    /// </summary>
    public void Push(IKeyboardControllable handler)
    {
        if (_isPushed || UI_KeyboardInputManager.Instance == null)
        {
            return;
        }

        UI_KeyboardInputManager.Instance.PushHandler(handler);
        _isPushed = true;
    }

    /// <summary>
    /// 등록에 성공했다면 캐시가 이미 채워져 있으므로, 해제 시점의 불필요한 씬 탐색을 피해
    /// HasInstance로 조회한다. 매니저가 이미 파괴된 경우에도 안전하다.
    /// </summary>
    public void Pop(IKeyboardControllable handler)
    {
        if (!_isPushed)
        {
            return;
        }

        _isPushed = false;
        if (UI_KeyboardInputManager.HasInstance)
        {
            UI_KeyboardInputManager.Instance.PopHandler(handler);
        }
    }
}
