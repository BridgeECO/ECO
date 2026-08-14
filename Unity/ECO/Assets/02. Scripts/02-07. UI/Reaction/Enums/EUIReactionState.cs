/// <summary>
/// 지속되는 UI 상태. Unity Selectable이 판정하는 상태를 그대로 따르되,
/// Selectable은 단일 상태만 주기 때문에 Hover와 Selected 동시 성립을 표현하려고 따로 추적한다.
///
/// 값은 직렬화되므로 재정렬하지 않는다. 상태 간 우선순위는 값이 아니라
/// UI_ReactionDispatcher가 별도 표로 정한다.
/// </summary>
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
