using UnityEngine;

public class AutoPlayInteraction : InteractionBase
{
    public AutoPlayInteraction(IInteractionTarget target) : base(target) { }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        TargetObject.Interact();
        TargetObject.SetState(true);
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        TargetObject.SetState(false);
    }
}

