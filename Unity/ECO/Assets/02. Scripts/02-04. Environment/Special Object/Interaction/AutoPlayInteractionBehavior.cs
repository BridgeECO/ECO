using UnityEngine;

public class AutoPlayInteractionBehavior : InteractionBehaviorBase
{
    public AutoPlayInteractionBehavior(SpecialObjectBase target) : base(target) { }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        TargetObject.CallInteract();
        TargetObject.CallSetState(true);
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        TargetObject.CallSetState(false);
    }
}
