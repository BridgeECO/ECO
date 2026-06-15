using System;
using R3;
using UnityEngine;

public class ButtonInteraction : InteractionBase
{
    private bool _isInteracting = false;
    private IDisposable _inputDisposable;

    public ButtonInteraction(SpecialObjectBase target) : base(target) { }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        _inputDisposable?.Dispose();
        _inputDisposable = Observable.EveryUpdate()
            .Where(_ => !InputHandler.IsInputBlocked)
            .Subscribe(_ =>
            {
                if (!_isInteracting && Input.GetKeyDown(KeyCode.F))
                {
                    Activate();
                }
                else if (_isInteracting && Input.GetKeyUp(KeyCode.F))
                {
                    Deactivate();
                }
            });
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        Deactivate();
        _inputDisposable?.Dispose();
    }

    public override void Dispose()
    {
        base.Dispose();
        Deactivate();
        _inputDisposable?.Dispose();
    }

    private void Activate()
    {
        _isInteracting = true;
        TargetObject.CallInteract();
    }

    private void Deactivate()
    {
        if (!_isInteracting)
        {
            return;
        }
        _isInteracting = false;
        TargetObject.CallSetState(false);
    }
}
