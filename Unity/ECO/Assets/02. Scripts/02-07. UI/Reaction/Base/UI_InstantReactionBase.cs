using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 시간 축이 없는 리액션의 공통 뼈대. 값을 한 번 바꾸고 물러날 때 곧바로 되돌린다.
/// </summary>
[Serializable]
public abstract class UI_InstantReactionBase : UI_ReactionBase
{
    public override bool IsPlaying => false;

    public override UniTask PlayAsync(UI_ReactionContext context,
        EUIReactionInterruptPolicy interruptPolicy, CancellationToken cancellationToken)
    {
        GameObject target = ResolveTarget(context);
        if (target != null)
        {
            Apply(context, target);
        }

        return UniTask.CompletedTask;
    }

    public override UniTask ExitAsync(UI_ReactionContext context,
        EUIReactionExitPolicy exitPolicy, CancellationToken cancellationToken)
    {
        // 보간할 값이 없어 TweenToBaseline도 즉시 복귀와 같다.
        if (exitPolicy != EUIReactionExitPolicy.Keep)
        {
            RestoreBaseline(context);
        }

        return UniTask.CompletedTask;
    }

    public override void Kill()
    {
    }

    public override void RestoreBaseline(UI_ReactionContext context)
    {
        GameObject target = ResolveTarget(context);
        if (target == null)
        {
            return;
        }

        Revert(context, target);
    }

    protected abstract void Apply(UI_ReactionContext context, GameObject target);

    protected abstract void Revert(UI_ReactionContext context, GameObject target);
}
