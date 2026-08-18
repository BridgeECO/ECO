using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// 대상의 anchoredPosition을 굴린다.
/// LayoutGroup 자식이나 ContentSizeFitter 대상은 레이아웃이 위치를 덮어쓰므로 자식을 하나 더 두고 움직인다.
/// </summary>
[Serializable]
[Preserve]
public class UI_PositionReaction : UI_TweenReactionBase
{
    [SerializeField]
    [Tooltip("기준값 기준 모드에서는 이동량으로, 절대값 모드에서는 목표 좌표로 쓰입니다.")]
    private Vector2 _position = new Vector2(0f, 8f);

    [SerializeField]
    private Vector2 _startPosition = Vector2.zero;

    public override EUIReactionChannel Channel => EUIReactionChannel.Position;

    protected override void ApplyStartValue(UI_ReactionContext context, GameObject target)
    {
        RectTransform rect = target.transform as RectTransform;
        if (rect == null)
        {
            return;
        }

        Vector2 baseline = EnsureBaseline(context, target, rect.anchoredPosition);

        if (IsUseStartValue)
        {
            rect.anchoredPosition = ResolveValue(_startPosition, baseline);
            return;
        }

        if (Motion.ValueMode == EUIReactionValueMode.FromBaseline)
        {
            rect.anchoredPosition = baseline;
        }
    }

    protected override Tween CreatePlayTween(UI_ReactionContext context, GameObject target)
    {
        RectTransform rect = target.transform as RectTransform;
        if (rect == null)
        {
            return null;
        }

        Vector2 baseline = EnsureBaseline(context, target, rect.anchoredPosition);
        return rect.DOAnchorPos(ResolveValue(_position, baseline), Motion.Duration);
    }

    protected override Tween CreateExitTween(UI_ReactionContext context, GameObject target)
    {
        RectTransform rect = target.transform as RectTransform;
        if (rect == null || !TryGetBaseline(context, target, out Vector4 baseline))
        {
            return null;
        }

        return rect.DOAnchorPos(baseline, Motion.Duration);
    }

    protected override void ApplyBaseline(UI_ReactionContext context, GameObject target)
    {
        RectTransform rect = target.transform as RectTransform;
        if (rect == null || !TryGetBaseline(context, target, out Vector4 baseline))
        {
            return;
        }

        rect.anchoredPosition = baseline;
    }

    private Vector2 ResolveValue(Vector2 value, Vector2 baseline)
    {
        if (Motion.ValueMode == EUIReactionValueMode.RelativeToBaseline)
        {
            return baseline + value;
        }

        return value;
    }
}
