using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class ButtonOneShotTimerInteraction : InteractionBase
{
    private readonly float _activeDuration;
    private bool _isInteracting = false;
    private IDisposable _inputDisposable;
    private CancellationTokenSource _cts;

    public ButtonOneShotTimerInteraction(IInteractionTarget target, float activeDuration) : base(target)
    {
        _activeDuration = activeDuration;
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        _inputDisposable?.Dispose();
        _inputDisposable = Observable.EveryUpdate()
            .Where(_ => !InputHandler.IsInputBlocked)
            .Subscribe(_ =>
            {
                if (!_isInteracting && Input.GetKeyDown(KeyCode.F))
                {
                    ActivateAsync().Forget();
                }
            });
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        _inputDisposable?.Dispose();
    }

    public override void Dispose()
    {
        base.Dispose();
        CancelTimer();
        _inputDisposable?.Dispose();
    }

    private async UniTaskVoid ActivateAsync()
    {
        _isInteracting = true;
        TargetObject.Interact();
        TargetObject.SetState(true);

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_activeDuration), cancellationToken: _cts.Token);
            if (TargetObject != null)
            {
                TargetObject.SetState(false);
            }
        }
        catch (OperationCanceledException)
        {
            // 痍⑥냼 ??臾댁떆
        }
        finally
        {
            _isInteracting = false;
        }
    }

    private void CancelTimer()
    {
        if (_cts is not null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            _isInteracting = false;
        }
    }
}

