/// <summary>
/// UI_KeyboardInputManager로부터 방향키/확인/취소 입력을 위임받는 핸들러 인터페이스.
/// 미사용 방향은 빈 구현으로 둔다. 배경 UI 차단은 Stack 최상단 원칙이 보장한다.
/// </summary>
public interface IKeyboardControllable
{
    public void OnMoveUp();

    public void OnMoveDown();

    public void OnMoveLeft();

    public void OnMoveRight();

    public void OnConfirm();

    public void OnCancel();
}
