using UnityEngine;

public class ButtonInteractionBehavior : InteractionBehaviorBase
{
    public ButtonInteractionBehavior(SpecialObjectBase target) : base(target) { }

    public override void OnUpdate()
    {
        if (TargetObject.IsPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            TargetObject.CallInteract();
        }
    }
}
