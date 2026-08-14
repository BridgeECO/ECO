using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

/// <summary>
/// 대상 Graphic의 색을 굴린다.
///
/// 주의: 버튼의 Transition이 ColorTint로 남아 있으면 Selectable이 CrossFadeColor로 같은 값을
/// 계속 덮어써 서로 싸운다. Transition을 None으로 두고 이 리액션으로 색을 관리한다.
/// </summary>
[Serializable]
[Preserve]
public class UI_ColorReaction : UI_TweenReactionBase
{
    [SerializeField]
    private Color _color = Color.white;

    [SerializeField]
    private Color _startColor = Color.white;

    public override EUIReactionChannel Channel => EUIReactionChannel.Color;

    protected override void ApplyStartValue(UI_ReactionContext context, GameObject target)
    {
        Graphic graphic = target.GetComponent<Graphic>();
        if (graphic == null)
        {
            return;
        }

        Color baseline = EnsureBaseline(context, target, ToVector(graphic.color));

        if (IsUseStartValue)
        {
            graphic.color = ResolveValue(_startColor, baseline);
            return;
        }

        if (Motion.ValueMode == EUIReactionValueMode.FromBaseline)
        {
            graphic.color = baseline;
        }
    }

    protected override Tween CreatePlayTween(UI_ReactionContext context, GameObject target)
    {
        Graphic graphic = target.GetComponent<Graphic>();
        if (graphic == null)
        {
            return null;
        }

        Color baseline = EnsureBaseline(context, target, ToVector(graphic.color));
        return graphic.DOColor(ResolveValue(_color, baseline), Motion.Duration);
    }

    protected override Tween CreateExitTween(UI_ReactionContext context, GameObject target)
    {
        Graphic graphic = target.GetComponent<Graphic>();
        if (graphic == null || !TryGetBaseline(context, target, out Vector4 baseline))
        {
            return null;
        }

        return graphic.DOColor(ToColor(baseline), Motion.Duration);
    }

    protected override void ApplyBaseline(UI_ReactionContext context, GameObject target)
    {
        Graphic graphic = target.GetComponent<Graphic>();
        if (graphic == null || !TryGetBaseline(context, target, out Vector4 baseline))
        {
            return;
        }

        graphic.color = ToColor(baseline);
    }

    private static Vector4 ToVector(Color color)
    {
        return new Vector4(color.r, color.g, color.b, color.a);
    }

    private static Color ToColor(Vector4 value)
    {
        return new Color(value.x, value.y, value.z, value.w);
    }

    // 색은 곱하기가 자연스럽다. 어두운 회색을 넣으면 원래 색이 그만큼 눌린다.
    private Color ResolveValue(Color value, Color baseline)
    {
        if (Motion.ValueMode == EUIReactionValueMode.RelativeToBaseline)
        {
            return baseline * value;
        }

        return value;
    }
}
