using System;
using UnityEngine;

public abstract class InteractionBase : IDisposable
{
    protected SpecialObjectBase TargetObject { get; private set; }

    public InteractionBase(SpecialObjectBase target)
    {
        TargetObject = target;
    }

    public virtual void OnTriggerEnter2D(Collider2D other) { }
    public virtual void OnTriggerExit2D(Collider2D other) { }

    public virtual void Dispose() { }
}
