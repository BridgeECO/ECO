using UnityEngine;

public class PressurePlateInteraction : InteractionBase
{
    public PressurePlateInteraction(IInteractionTarget target) : base(target) { }

    public override string ValidTag => nameof(ETags.PlayerFeet);

    public override void OnTriggerEnter2D(Collider2D other)
    {
        TargetObject.SetState(true);
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        TargetObject.SetState(false);
    }
}

