using UnityEngine;

public class AutoPlayInteraction : InteractionBase
{
    public AutoPlayInteraction(SpecialObjectBase target) : base(target) { }

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
