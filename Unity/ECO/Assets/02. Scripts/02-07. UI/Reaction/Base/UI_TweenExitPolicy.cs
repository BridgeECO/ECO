using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 지속 상태가 끝나 물러날 때 무엇을 할지 정하고 실행한다.
/// 이 처리가 없으면 마우스를 빠르게 움직였을 때 연출이 중간값에 멈춘 채 남는다.
/// </summary>
public static class UI_TweenExitPolicy
{
    public static UniTask ExitAsync(IUITweenReaction reaction, UI_ReactionContext context,
        EUIReactionExitPolicy exitPolicy, CancellationToken cancellationToken)
    {
        if (exitPolicy == EUIReactionExitPolicy.Keep)
        {
            return UniTask.CompletedTask;
        }

        GameObject target = reaction.ResolveTarget(context);
        if (target == null)
        {
            return UniTask.CompletedTask;
        }

        if (exitPolicy == EUIReactionExitPolicy.Reverse && reaction.Runner.TryReverse())
        {
            return reaction.Runner.WaitAsync(cancellationToken);
        }

        // 무한 반복에는 "끝까지"가 없다. 기다리면 기준값으로 영영 돌아오지 않으므로
        // 그때는 아래 즉시 복귀로 흘려보낸다.
        if (exitPolicy == EUIReactionExitPolicy.PlayToEnd && !reaction.IsInfiniteLoop)
        {
            return RestoreAfterCurrentAsync(reaction, context, cancellationToken);
        }

        if (exitPolicy == EUIReactionExitPolicy.TweenToBaseline)
        {
            reaction.Runner.Run(reaction.ApplyExitMotion(reaction.CreateExitTween(context, target)), target);
            return reaction.Runner.WaitAsync(cancellationToken);
        }

        reaction.RestoreBaseline(context);
        return UniTask.CompletedTask;
    }

    private static async UniTask RestoreAfterCurrentAsync(IUITweenReaction reaction, UI_ReactionContext context,
        CancellationToken cancellationToken)
    {
        await reaction.Runner.WaitAsync(cancellationToken);
        if (!cancellationToken.IsCancellationRequested)
        {
            reaction.RestoreBaseline(context);
        }
    }
}
