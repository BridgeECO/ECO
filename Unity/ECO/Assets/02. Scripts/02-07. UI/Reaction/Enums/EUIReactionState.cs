/// <summary>
/// 지속되는 UI 상태. 상태 간 우선순위는 이 값이 아니라 UI_ReactionStateArbiter가 별도 표로 정한다.
/// </summary>
// 값이 직렬화되므로 재정렬하지 않는다.
public enum EUIReactionState
{
    Normal = 0,

    /// <summary>포인터가 UI 위에 올라와 있다.</summary>
    Hover = 1,

    /// <summary>누르고 있다.</summary>
    Pressed = 2,

    /// <summary>EventSystem의 현재 Navigation 대상이다. Hover와 별개다.</summary>
    Selected = 3,

    /// <summary>Selectable.IsInteractable()이 false다.</summary>
    Disabled = 4,
}
