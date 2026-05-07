using System;
using UnityEngine;

public abstract class InteractionBase : IDisposable
{
    protected SpecialObjectBase TargetObject { get; private set; }

    public InteractionBase(SpecialObjectBase target)
    {
        TargetObject = target;
    }

    public abstract void OnTriggerEnter2D(Collider2D other);
    public abstract void OnTriggerExit2D(Collider2D other);
    public virtual void Dispose() { }
}
