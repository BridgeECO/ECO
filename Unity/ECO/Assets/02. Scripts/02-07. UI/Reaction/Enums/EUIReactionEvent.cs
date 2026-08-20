/// <summary>
/// EventSystem이 보내는 단발 입력. 값은 직렬화되므로 재정렬하지 않는다.
/// </summary>
public enum EUIReactionEvent
{
    PointerEnter = 0,
    PointerExit = 1,
    PointerDown = 2,
    PointerUp = 3,

    /// <summary>포인터 클릭만. 키보드 Submit은 포함하지 않는다.</summary>
    Click = 4,

    /// <summary>키보드·게임패드 확정만. 포인터 클릭은 포함하지 않는다.</summary>
    Submit = 5,

    /// <summary>
    /// 클릭 또는 Submit. 둘 다 "버튼을 실행했다"는 같은 의미라
    /// 기획자가 양쪽에 같은 연출을 중복 등록하지 않도록 하나로 묶는다.
    /// </summary>
    Activate = 6,

    Select = 7,
    Deselect = 8,
    Cancel = 9,
}
