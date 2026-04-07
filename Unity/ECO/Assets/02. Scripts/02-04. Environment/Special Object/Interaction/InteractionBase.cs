using UnityEngine;

public abstract class InteractionBase
{
    protected SpecialObjectBase TargetObject { get; private set; }

    public InteractionBase(SpecialObjectBase target)
    {
        TargetObject = target;
    }

    public virtual void OnUpdate() { }
    public virtual void OnTriggerEnter2D(Collider2D other) { }
    public virtual void OnTriggerExit2D(Collider2D other) { }
}
