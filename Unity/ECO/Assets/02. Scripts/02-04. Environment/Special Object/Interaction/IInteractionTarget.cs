/// <summary>
/// Interaction이 대상 오브젝트에 요구하는 최소 계약.
/// Interaction이 SpecialObjectBase 구체 타입을 역참조하지 않도록 격리하며,
/// protected 멤버를 우회하던 공개 래퍼(CallInteract/CallSetState)를 대체한다.
/// </summary>
public interface IInteractionTarget
{
    void Interact();
    void SetState(bool isOn);
}
