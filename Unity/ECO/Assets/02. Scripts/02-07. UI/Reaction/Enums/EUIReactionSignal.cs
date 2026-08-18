/// <summary>
/// 게임 코드가 UI_Reactor.PlaySignalAsync로 직접 발동하는 연출 신호(팝업 개폐, 피격, 알림 등퇴장).
/// </summary>
// 값이 직렬화되므로 항목은 뒤에만 추가한다. 순서를 바꾸면 저장된 항목이 다른 신호를 가리킨다.
public enum EUIReactionSignal
{
    /// <summary>등장. UI_Reactor의 옵션에 따라 OnEnable에서 자동 재생될 수 있다.</summary>
    Show = 0,

    /// <summary>퇴장. 호출부가 await로 끝까지 기다린 뒤 비활성화해야 한다.</summary>
    Hide = 1,

    Damaged = 2,
    Healed = 3,
}
