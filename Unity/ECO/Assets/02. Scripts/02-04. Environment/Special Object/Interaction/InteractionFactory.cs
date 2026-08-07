/// <summary>
/// EInteractionType에 대응하는 Interaction 구현체를 생성하는 정적 팩토리.
/// 추상 베이스(SpecialObjectBase)가 파생 Interaction 구체 타입들을 직접 알지 않도록 분리한다.
/// 새 Interaction 추가 시 이 팩토리만 수정하면 된다.
/// </summary>
public static class InteractionFactory
{
    public static InteractionBase Create(EInteractionType interactionType, IInteractionTarget target, float buttonOneShotDuration)
    {
        switch (interactionType)
        {
            case EInteractionType.ButtonPressAndHold:
                return new ButtonPressAndHoldInteraction(target);
            case EInteractionType.ButtonOneShotTimer:
                return new ButtonOneShotTimerInteraction(target, buttonOneShotDuration);
            case EInteractionType.AutoPlay:
                return new AutoPlayInteraction(target);
            case EInteractionType.PressurePlate:
                return new PressurePlateInteraction(target);
            default:
                return null;
        }
    }
}
