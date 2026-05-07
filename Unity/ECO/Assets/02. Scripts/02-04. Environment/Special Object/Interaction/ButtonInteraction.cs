using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class ButtonInteraction : InteractionBase
{
    private readonly float _deactivateDelayTime;
    private bool _isInteracting = false;
    private IDisposable _inputDisposable;
    private CancellationTokenSource _delayCts;

    public ButtonInteraction(SpecialObjectBase target) : base(target)
    {
        ButtonInteractionData data = target.GetComponent<ButtonInteractionData>();
        _deactivateDelayTime = data != null ? data.DeactivateDelayTime : 2f;
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        _inputDisposable?.Dispose();
        _inputDisposable = Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.F))
            .Subscribe(_ =>
            {
                if (!_isInteracting)
                {
                    InteractAsync().Forget();
                }
                else
                {
                    CancelInteraction();
                }
            });
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        base.OnTriggerExit2D(other);
        _inputDisposable?.Dispose();
    }

    public override void Dispose()
    {
        base.Dispose();
        _inputDisposable?.Dispose();
        _delayCts?.Cancel();
        _delayCts?.Dispose();
    }

    private async UniTaskVoid InteractAsync()
    {
        _isInteracting = true;
        TargetObject.CallInteract();

        _delayCts?.Cancel();
        _delayCts?.Dispose();
        _delayCts = CancellationTokenSource.CreateLinkedTokenSource(TargetObject.GetCancellationTokenOnDestroy());

        bool isCancelled = await UniTask.Delay(System.TimeSpan.FromSeconds(_deactivateDelayTime), cancellationToken: _delayCts.Token).SuppressCancellationThrow();

        if (isCancelled)
        {
            return;
        }

        TargetObject.CallSetState(false);
        _isInteracting = false;
    }

    private void CancelInteraction()
    {
        _delayCts?.Cancel();
        TargetObject.CallInteract();
        TargetObject.CallSetState(false);
        _isInteracting = false;
    }
}
