/// <summary>
/// 플레이어 상호작용 선택기(PlayerInteractTargetSelector)에게만 노출되는 선택 후보 진입점.
/// SpecialObjectBase가 명시적 인터페이스로 구현해 일반 참조로는 선택 상태를 바꿀 수 없게 한다.
/// </summary>
public interface IInteractionCandidate
{
    bool IsEngaged { get; }

    void SetSelected(bool isSelected);
}
