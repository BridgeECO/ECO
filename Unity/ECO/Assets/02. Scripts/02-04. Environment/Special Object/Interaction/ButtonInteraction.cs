using UnityEngine;

public class ButtonInteraction : InteractionBase
{
    public ButtonInteraction(SpecialObjectBase target) : base(target) { }

    public override void OnUpdate()
    {
        if (TargetObject.IsPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            TargetObject.CallInteract();
        }
    }
}
