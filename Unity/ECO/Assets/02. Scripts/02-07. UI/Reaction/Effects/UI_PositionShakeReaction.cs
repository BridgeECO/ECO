using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// anchoredPosition을 랜덤하게 떨리게 한다. LayoutGroup 자식이나 ContentSizeFitter 대상은
/// 레이아웃이 위치를 덮어쓰므로 자식을 하나 더 두고 떨게 한다.
/// </summary>
[Serializable]
[Preserve]
public class UI_PositionShakeReaction : UI_ShakeReactionBase
{
    [SerializeField]
    [Tooltip("떨림의 진폭입니다(픽셀). 기준 좌표에 더해지는 절대량입니다.")]
    private Vector2 _strength = new Vector2(8f, 8f);

    public override EUIReactionChannel Channel => EUIReactionChannel.Position;

    protected override void ApplyStartValue(UI_ReactionContext context, GameObject target)
    {
        RectTransform rect = target.transform as RectTransform;
        if (rect == null)
        {
            return;
        }

        Vector2 baseline = EnsureBaseline(context, target, rect.anchoredPosition);

        if (IsStartFromBaseline)
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

        EnsureBaseline(context, target, rect.anchoredPosition);
        return rect.DOShakeAnchorPos(Motion.Duration, _strength, Vibrato, Randomness, false, IsFadeOut);
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
}
