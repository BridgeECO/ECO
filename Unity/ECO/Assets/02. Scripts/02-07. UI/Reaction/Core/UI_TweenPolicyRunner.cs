using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 트윈 리액션 하나의 핸들 수명을 맡는다.
///
/// DOKill()은 그 대상의 모든 트윈을 죽여 Scale 리액션이 Position 리액션까지 끊어 버린다.
/// 그래서 여기서는 자기 핸들만 들고 Kill한다. 외부 코드가 DOKill을 부르더라도 OnKill로
/// 핸들을 비워 두므로 죽은 트윈을 붙들지 않는다.
///
/// AutoKill을 끄는 이유는 되감기 때문이다. Hover가 끝난 뒤 되돌리려면 이미 완료된 트윈이
/// 살아 있어야 SmoothRewind를 걸 수 있다. 대신 반드시 Kill로 직접 정리한다.
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

        _tween = tween
            .SetAutoKill(false)
            .SetLink(linkTarget, LinkBehaviour.KillOnDestroy)
            .OnComplete(_onFinished)
            .OnRewind(_onFinished)
            .OnKill(_onKilled);
    }

    /// <summary>
    /// 재생이 끝날 때까지 기다린다. 아무도 기다리지 않으면 완료 소스를 만들지 않으므로
    /// 상태 트리거처럼 던져 놓고 잊는 경로에서는 할당이 생기지 않는다.
    /// </summary>
    public UniTask WaitAsync()
    {
        if (!IsPlaying)
        {
            return UniTask.CompletedTask;
        }

        if (_completion == null)
        {
            _completion = new UniTaskCompletionSource();
        }

        return _completion.Task;
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
