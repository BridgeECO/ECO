using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// 대상의 크기를 굴린다. localScale은 Layout이 덮어쓰지 않아 LayoutGroup 자식에도 안전하다.
/// </summary>
[Serializable]
[Preserve]
public class UI_ScaleReaction : UI_TweenReactionBase
{
    [SerializeField]
    [Tooltip("기준값 기준 모드에서는 배율로, 절대값 모드에서는 그대로 쓰입니다.")]
    private Vector3 _scale = new Vector3(1.1f, 1.1f, 1f);

    [SerializeField]
    private Vector3 _startScale = new Vector3(0.5f, 0.5f, 0.5f);

    public override EUIReactionChannel Channel => EUIReactionChannel.Scale;

    protected override void ApplyStartValue(UI_ReactionContext context, GameObject target)
    {
        Vector3 baseline = EnsureBaseline(context, target, target.transform.localScale);

        if (IsUseStartValue)
        {
            target.transform.localScale = ResolveValue(_startScale, baseline);
            return;
        }

        if (Motion.ValueMode == EUIReactionValueMode.FromBaseline)
        {
            target.transform.localScale = baseline;
        }
    }

    protected override Tween CreatePlayTween(UI_ReactionContext context, GameObject target)
    {
        Vector3 baseline = EnsureBaseline(context, target, target.transform.localScale);
        return target.transform.DOScale(ResolveValue(_scale, baseline), Motion.Duration);
    }

    protected override Tween CreateExitTween(UI_ReactionContext context, GameObject target)
    {
        if (!TryGetBaseline(context, target, out Vector4 baseline))
        {
            return null;
        }

        return target.transform.DOScale((Vector3)baseline, Motion.Duration);
    }

    protected override void ApplyBaseline(UI_ReactionContext context, GameObject target)
    {
        if (!TryGetBaseline(context, target, out Vector4 baseline))
        {
            return;
        }

        target.transform.localScale = baseline;
    }

    // 크기는 더하기보다 곱하기가 직관적이다. 1.1을 넣으면 어느 원본 크기에서든 10% 커진다.
    private Vector3 ResolveValue(Vector3 value, Vector3 baseline)
    {
        if (Motion.ValueMode == EUIReactionValueMode.RelativeToBaseline)
        {
            return Vector3.Scale(baseline, value);
        }

        return value;
    }
}
