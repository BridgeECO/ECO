using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 재생이 끝나기 전에 같은 리액션이 다시 요청됐을 때 무엇을 할지 정하고 실행한다.
/// 트윈 핸들의 수명은 UI_TweenPolicyRunner가 따로 맡는다.
/// </summary>
public static class UI_TweenPlayPolicy
{
    // async를 붙이지 않는다. hover가 오갈 때마다 지나가는 경로라 상태머신 객체가 그대로 쓰레기가 된다.
    // 기다림이 필요한 드문 정책만 아래 async 헬퍼로 빠진다.
    public static UniTask PlayAsync(IUITweenReaction reaction, UI_ReactionContext context,
        EUIReactionInterruptPolicy interruptPolicy, CancellationToken cancellationToken)
    {
        GameObject target = reaction.ResolveTarget(context);
        if (target == null)
        {
            return UniTask.CompletedTask;
        }

        // 되감기는 재생 중일 때만 성립한다. 완료된 트윈은 AutoKill을 꺼 둔 탓에 살아 있을 뿐이라,
        // 이걸 되감으면 새로 요청한 재생이 거꾸로 도는 꼴이 된다.
        // 되감기가 끝날 때까지 기다린다. 즉시 완료로 돌려주면 자리 해제를 태스크 완료에 묶어 둔
        // 이벤트 경로가 아직 도는 트윈을 두고 자리를 놓고, 신호 경로는 팝업을 그대로 꺼 버린다.
        if (interruptPolicy == EUIReactionInterruptPolicy.Reverse && reaction.Runner.IsPlaying)
        {
            return reaction.Runner.TryReverse()
                ? reaction.Runner.WaitAsync(cancellationToken)
                : UniTask.CompletedTask;
        }

        if (!TryEnterPlay(interruptPolicy, reaction.Runner))
        {
            return UniTask.CompletedTask;
        }

        // 무한 반복에는 "끝까지"가 없다. 기다리면 이 리액션이 다시는 재생되지 않으므로
        // 그때는 Restart와 같게 곧바로 새로 재생한다.
        if (interruptPolicy == EUIReactionInterruptPolicy.PlayToEndThenPlay
            && reaction.Runner.IsPlaying && !reaction.IsInfiniteLoop)
        {
            return PlayAfterCurrentAsync(reaction, context, target, cancellationToken);
        }

        return StartAsync(reaction, context, target, cancellationToken);
    }

    private static UniTask StartAsync(IUITweenReaction reaction, UI_ReactionContext context, GameObject target,
        CancellationToken cancellationToken)
    {
        reaction.ApplyStartValue(context, target);
        reaction.Runner.Run(reaction.ApplyPlayMotion(reaction.CreatePlayTween(context, target)), target);

        // 무한 반복은 끝나지 않으므로 기다리면 호출부가 영영 풀리지 않는다.
        return reaction.IsInfiniteLoop ? UniTask.CompletedTask : reaction.Runner.WaitAsync(cancellationToken);
    }

    private static async UniTask PlayAfterCurrentAsync(IUITweenReaction reaction, UI_ReactionContext context,
        GameObject target, CancellationToken cancellationToken)
    {
        await reaction.Runner.WaitAsync(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        await StartAsync(reaction, context, target, cancellationToken);
    }

    // 정책상 새 재생을 시작해도 되는지. 대기가 필요한 정책은 호출부에서 따로 처리한다.
    private static bool TryEnterPlay(EUIReactionInterruptPolicy interruptPolicy, UI_TweenPolicyRunner runner)
    {
        switch (interruptPolicy)
        {
            // 리액션마다 트윈 핸들을 따로 들기 때문에 "같은 연출이 이미 돌고 있다"는 곧
            // "이 핸들이 돌고 있다"와 같다. 그래서 두 정책은 이 구조에서 결과가 같다.
            // 저장된 값과의 호환 때문에 열거형에는 둘 다 남겨 둔다.
            case EUIReactionInterruptPolicy.IgnoreUntilDone:
            case EUIReactionInterruptPolicy.SkipIfSame:
                return !runner.IsPlaying;

            // Reverse는 되감기를 기다려야 해서 PlayAsync가 앞에서 직접 처리한다.
            // 여기까지 왔다면 재생 중이 아니라는 뜻이라 평범하게 새로 재생한다.
            default:
                return true;
        }
    }
}
