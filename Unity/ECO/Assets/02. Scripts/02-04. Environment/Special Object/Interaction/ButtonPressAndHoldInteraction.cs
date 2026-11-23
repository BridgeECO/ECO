using System;
using R3;
using UnityEngine;

public class ButtonPressAndHoldInteraction : ButtonInteractionBase
{
    private bool _isInteracting = false;
    private IDisposable _inputDisposable;

    // 입력이 막힌 동안 뗀 F는 구독 필터에 걸려 _isInteracting이 남을 수 있어, 실제로 누르고 있는지도 함께 본다.
    public override bool IsEngaged => _isInteracting && Input.GetKey(KeyCode.F);

    public ButtonPressAndHoldInteraction(IInteractionTarget target) : base(target) { }

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
        TargetObject.Interact();
    }

    private void Deactivate()
    {
        if (!_isInteracting)
        {
            return;
        }
        _isInteracting = false;
        TargetObject.SetState(false);
    }
}

