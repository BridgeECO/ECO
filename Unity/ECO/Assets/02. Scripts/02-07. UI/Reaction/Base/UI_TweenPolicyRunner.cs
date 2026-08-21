using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 트윈 리액션 하나의 핸들 수명을 맡는다. DOKill은 대상의 모든 트윈을 죽여 남의 리액션까지
/// 끊으므로 자기 핸들만 들고 Kill한다.
/// </summary>
public class UI_TweenPolicyRunner
{
    private readonly TweenCallback _onFinished;
    private readonly TweenCallback _onKilled;

    private Tween _tween;
    private UniTaskCompletionSource _completion;

    public UI_TweenPolicyRunner()
    {
        // 람다로 넘기면 재생할 때마다 델리게이트가 새로 잡히므로 한 번만 만들어 재사용한다.
        _onFinished = SignalCompletion;
        _onKilled = HandleKilled;
    }

    public bool IsPlaying => _tween != null && _tween.IsActive() && _tween.IsPlaying();

    /// <summary>완료됐어도 되감기용으로 살아 있는 상태를 포함한다.</summary>
    public bool HasTween => _tween != null && _tween.IsActive();

    public void Run(Tween tween, GameObject linkTarget)
    {
        Kill(false);

        if (tween == null)
        {
            return;
        }

        // 되감기를 걸려면 완료된 트윈이 살아 있어야 해 AutoKill을 끈다. 대신 반드시 Kill로 직접 정리한다.
        _tween = tween
            .SetAutoKill(false)
            .SetLink(linkTarget, LinkBehaviour.KillOnDestroy)
            .OnComplete(_onFinished)
            .OnRewind(_onFinished)
            .OnKill(_onKilled);
    }

    /// <summary>기다리는 쪽이 없으면 완료 소스를 만들지 않아, 던져 놓고 잊는 경로에는 할당이 없다.</summary>
    public UniTask WaitAsync(CancellationToken cancellationToken)
    {
        if (!IsPlaying || cancellationToken.IsCancellationRequested)
        {
            return UniTask.CompletedTask;
        }

        if (_completion == null)
        {
            _completion = new UniTaskCompletionSource();
        }

        // 토큰을 걸지 않으면 대기가 트윈의 완료·중단 신호에만 매달린다. 그 신호가 한 번이라도
        // 빠지면 기다리던 쪽이 영영 풀리지 않아, Hide를 await하던 팝업이 열린 채로 남는다.
        // 취소는 정상 종료로 다루므로 예외를 던지지 않는다.
        return cancellationToken.CanBeCanceled
            ? WaitCoreAsync(_completion.Task, cancellationToken)
            : _completion.Task;
    }

    private static async UniTask WaitCoreAsync(UniTask task, CancellationToken cancellationToken)
    {
        await task.AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();
    }

    /// <summary>현재 지점에서 시작값으로 되감는다. 되감을 트윈이 없으면 false.</summary>
    public bool TryReverse()
    {
        if (!HasTween)
        {
            return false;
        }

        _tween.SmoothRewind();
        return true;
    }

    public void Kill(bool isComplete)
    {
        if (_tween == null)
        {
            return;
        }

        // Kill이 부르는 OnKill 안에서 다시 이 필드를 건드리므로 먼저 비운다.
        Tween tween = _tween;
        _tween = null;
        tween.Kill(isComplete);
    }

    private void HandleKilled()
    {
        _tween = null;
        SignalCompletion();
    }

    // OnComplete 뒤에 OnKill이 또 오므로 두 번 불릴 수 있다. 소스를 먼저 떼어 한 번만 신호한다.
    private void SignalCompletion()
    {
        UniTaskCompletionSource completion = _completion;
        if (completion == null)
        {
            return;
        }

        _completion = null;
        completion.TrySetResult();
    }
}
