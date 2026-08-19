/// <summary>
/// 리액션 항목이 무엇에 반응하는지 구분한다.
/// 지속 상태와 단발 이벤트는 수명이 달라(복귀 처리 유무) 한 열거형으로 섞지 않는다.
/// </summary>
public enum EUIReactionTriggerKind
{
    /// <summary>포인터·선택 상태가 유지되는 동안 지속. 종료 시 복귀 처리가 필요하다.</summary>
    State = 0,

    /// <summary>EventSystem이 보내는 1회성 입력. 복귀 개념이 없다.</summary>
    Event = 1,

    /// <summary>게임 코드가 명시적으로 요청하는 1회성 연출(팝업 개폐, 피격 등).</summary>
    Signal = 2,
}
