using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// DOTween으로 값을 굴리는 리액션의 공통 뼈대.
/// 트윈 수명과 기준값만 맡고, 중단·복귀 정책의 해석은 정책 클래스에 넘긴다.
/// </summary>
[Serializable]
public abstract class UI_TweenReactionBase : UI_ReactionBase, IUITweenReaction
{
    [SerializeField]
    private UI_MotionSettings _motion = new UI_MotionSettings();

    [SerializeField]
    [Tooltip("체크하면 재생 직전에 시작값으로 먼저 옮긴 뒤 목표값으로 굴립니다. 팝업 개폐처럼 항상 같은 지점에서 출발해야 하는 연출에 씁니다.")]
    private bool _isUseStartValue = false;

    private UI_TweenPolicyRunner _runner;

    public override bool IsPlaying => Runner.IsPlaying;

    protected UI_MotionSettings Motion => _motion;

    protected bool IsUseStartValue => _isUseStartValue;

    // [SerializeReference] 역직렬화 경로에서 초기화가 도는지에 기대지 않고 지연 생성한다.
    protected UI_TweenPolicyRunner Runner
    {
        get
        {
            if (_runner == null)
            {
                _runner = new UI_TweenPolicyRunner();
            }

            return _runner;
        }
    }

    #region Logic
    public override UniTask PlayAsync(UI_ReactionContext context,
        EUIReactionInterruptPolicy interruptPolicy, CancellationToken cancellationToken)
    {
        return UI_TweenPlayPolicy.PlayAsync(this, context, interruptPolicy, cancellationToken);
    }

    public override UniTask ExitAsync(UI_ReactionContext context,
        EUIReactionExitPolicy exitPolicy, CancellationToken cancellationToken)
    {
        return UI_TweenExitPolicy.ExitAsync(this, context, exitPolicy, cancellationToken);
    }

    public override void Kill()
    {
        Runner.Kill(false);
    }

    public override void RestoreBaseline(UI_ReactionContext context)
    {
        Runner.Kill(false);

        GameObject target = ResolveTarget(context);
        if (target == null)
        {
            return;
        }

        ApplyBaseline(context, target);
    }

    protected Vector4 EnsureBaseline(UI_ReactionContext context, GameObject target, Vector4 currentValue)
    {
        return EnsureBaseline(context, target, currentValue, null, false).Value;
    }

    protected bool TryGetBaseline(UI_ReactionContext context, GameObject target, out Vector4 value)
    {
        if (TryGetBaseline(context, target, out UI_ReactionBaseline stored))
        {
            value = stored.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>재생 직전에 출발 지점을 맞춘다. 시작값을 쓰지 않고 기준값에서 출발하는 모드도 여기서 처리한다.</summary>
    protected abstract void ApplyStartValue(UI_ReactionContext context, GameObject target);

    protected abstract Tween CreatePlayTween(UI_ReactionContext context, GameObject target);

    protected abstract Tween CreateExitTween(UI_ReactionContext context, GameObject target);

    protected abstract void ApplyBaseline(UI_ReactionContext context, GameObject target);
    #endregion

    // 정책 클래스만 쓰는 통로다. 명시적 구현으로 두어 파생 클래스의 접근 수준을 건드리지 않는다.
    #region IUITweenReaction
    UI_TweenPolicyRunner IUITweenReaction.Runner => Runner;

    bool IUITweenReaction.IsInfiniteLoop => _motion.IsInfiniteLoop;

    void IUITweenReaction.ApplyStartValue(UI_ReactionContext context, GameObject target)
        => ApplyStartValue(context, target);

    Tween IUITweenReaction.CreatePlayTween(UI_ReactionContext context, GameObject target)
        => CreatePlayTween(context, target);

    Tween IUITweenReaction.CreateExitTween(UI_ReactionContext context, GameObject target)
        => CreateExitTween(context, target);

    Tween IUITweenReaction.ApplyPlayMotion(Tween tween) => _motion.ApplyTo(tween);

    Tween IUITweenReaction.ApplyExitMotion(Tween tween) => _motion.ApplyToExit(tween);
    #endregion
}
