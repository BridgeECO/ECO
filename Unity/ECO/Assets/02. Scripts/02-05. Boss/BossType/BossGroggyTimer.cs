using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public sealed class BossGroggyTimer
{
    private CancellationTokenSource _cancellationTokenSource;

    public async UniTask<bool> WaitAsync(float duration, CancellationToken cancellationToken)
    {
        Cancel();
        CancellationTokenSource timerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _cancellationTokenSource = timerCts;

        try
        {
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(duration),
                cancellationToken: timerCts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            if (ReferenceEquals(_cancellationTokenSource, timerCts))
            {
                _cancellationTokenSource = null;
            }

            timerCts.Dispose();
        }
    }

    public void Cancel()
    {
        if (_cancellationTokenSource is null)
        {
            return;
        }

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
    }
}
