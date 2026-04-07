using UnityEngine;

public class PressurePlateInteraction : InteractionBase
{
    public PressurePlateInteraction(SpecialObjectBase target) : base(target) { }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        TargetObject.CallSetState(true);
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        TargetObject.CallSetState(false);
    }
}
