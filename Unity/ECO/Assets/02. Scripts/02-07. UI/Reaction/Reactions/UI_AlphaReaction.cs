using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

/// <summary>
/// 대상의 투명도를 굴린다.
/// CanvasGroup이 있으면 그쪽을 쓴다. Graphic.color는 트윈이 도는 내내 메시를 다시 만든다.
/// </summary>
[Serializable]
[Preserve]
public class UI_AlphaReaction : UI_TweenReactionBase
{
    [SerializeField]
    [Range(0f, 1f)]
    private float _alpha = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    private float _startAlpha = 0f;

    public override EUIReactionChannel Channel => EUIReactionChannel.Alpha;

    protected override void ApplyStartValue(UI_ReactionContext context, GameObject target)
    {
        if (!TryResolve(target, out CanvasGroup canvasGroup, out Graphic graphic))
        {
            return;
        }

        float baseline = EnsureBaseline(context, target, new Vector4(ReadAlpha(canvasGroup, graphic), 0f, 0f, 0f)).x;

        if (IsUseStartValue)
        {
            WriteAlpha(canvasGroup, graphic, ResolveValue(_startAlpha, baseline));
            return;
        }

        if (Motion.ValueMode == EUIReactionValueMode.FromBaseline)
        {
            WriteAlpha(canvasGroup, graphic, baseline);
        }
    }

    protected override Tween CreatePlayTween(UI_ReactionContext context, GameObject target)
    {
        if (!TryResolve(target, out CanvasGroup canvasGroup, out Graphic graphic))
        {
            return null;
        }

        float baseline = EnsureBaseline(context, target, new Vector4(ReadAlpha(canvasGroup, graphic), 0f, 0f, 0f)).x;
        float end = ResolveValue(_alpha, baseline);

        return canvasGroup != null
            ? canvasGroup.DOFade(end, Motion.Duration)
            : graphic.DOFade(end, Motion.Duration);
    }

    protected override Tween CreateExitTween(UI_ReactionContext context, GameObject target)
    {
        if (!TryResolve(target, out CanvasGroup canvasGroup, out Graphic graphic)
            || !TryGetBaseline(context, target, out Vector4 baseline))
        {
            return null;
        }

        return canvasGroup != null
            ? canvasGroup.DOFade(baseline.x, Motion.Duration)
            : graphic.DOFade(baseline.x, Motion.Duration);
    }

    protected override void ApplyBaseline(UI_ReactionContext context, GameObject target)
    {
        if (!TryResolve(target, out CanvasGroup canvasGroup, out Graphic graphic)
            || !TryGetBaseline(context, target, out Vector4 baseline))
        {
            return;
        }

        WriteAlpha(canvasGroup, graphic, baseline.x);
    }

    private static bool TryResolve(GameObject target, out CanvasGroup canvasGroup, out Graphic graphic)
    {
        canvasGroup = target.GetComponent<CanvasGroup>();
        graphic = canvasGroup != null ? null : target.GetComponent<Graphic>();
        return canvasGroup != null || graphic != null;
    }

    private static float ReadAlpha(CanvasGroup canvasGroup, Graphic graphic)
    {
        return canvasGroup != null ? canvasGroup.alpha : graphic.color.a;
    }

    private static void WriteAlpha(CanvasGroup canvasGroup, Graphic graphic, float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            return;
        }

        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    // 투명도는 곱하기가 자연스럽다. 0.5를 넣으면 원래 값이 얼마든 절반이 된다.
    private float ResolveValue(float value, float baseline)
    {
        if (Motion.ValueMode == EUIReactionValueMode.RelativeToBaseline)
        {
            return Mathf.Clamp01(baseline * value);
        }

        return value;
    }
}
