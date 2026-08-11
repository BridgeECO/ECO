using System;
using R3;
using UnityEngine;

/// <summary>
/// 상호작용 키를 누를 때마다 활성/비활성이 뒤바뀌는 인터랙션.
/// 키를 떼도 상태가 유지되므로, 플레이어가 이동하는 동안 계속 유지되어야 하는 연출에 사용한다.
/// </summary>
public class ButtonToggleInteraction : InteractionBase
{
    private bool _isInteracting = false;
    private IDisposable _inputDisposable;

    public ButtonToggleInteraction(IInteractionTarget target) : base(target) { }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        _inputDisposable?.Dispose();
        _inputDisposable = Observable.EveryUpdate()
            .Where(_ => !InputHandler.IsInputBlocked)
            .Subscribe(_ =>
            {
                if (!Input.GetKeyDown(KeyCode.F))
                {
                    return;
                }

                if (_isInteracting)
                {
                    Deactivate();
                }
                else
                {
                    Activate();
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

    // 다른 Interaction과 달리 SetState(true)를 호출하지 않는다.
    // EnergySwitch처럼 Interact()가 곧 토글인 대상에서는 SetState(true)가 뒤따르면
    // 끄려고 누른 입력이 곧바로 다시 켜 버린다.
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
