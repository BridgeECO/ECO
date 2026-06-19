using UnityEngine;

public class PressurePlateInteraction : InteractionBase
{
    public PressurePlateInteraction(SpecialObjectBase target) : base(target) { }
    
    public override string ValidTag => nameof(ETags.PlayerFeet);

    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(nameof(ETags.PlayerFeet)))
        {
            return;
        }
        TargetObject.CallSetState(true);
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(nameof(ETags.PlayerFeet)))
        {
            return;
        }
        TargetObject.CallSetState(false);
    }
}
