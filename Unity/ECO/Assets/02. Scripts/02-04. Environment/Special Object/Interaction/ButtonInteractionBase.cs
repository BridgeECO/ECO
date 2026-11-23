/// <summary>
/// 버튼식 인터랙션의 공통 베이스. 플레이어 상호작용 범위(PlayerInteract 태그)만 받고,
/// 범위 안 후보 중 선택된 하나만 F 입력을 받도록 선택을 거친다.
/// </summary>
public abstract class ButtonInteractionBase : InteractionBase
{
    protected ButtonInteractionBase(IInteractionTarget target) : base(target) { }

    public override string ValidTag => nameof(ETags.PlayerInteract);
    public override bool IsSelectionRequired => true;
}
