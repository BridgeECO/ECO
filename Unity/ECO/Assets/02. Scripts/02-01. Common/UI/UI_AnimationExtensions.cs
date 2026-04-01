using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class UI_AnimationExtensions
{
    private static Tween AddOnCompleteActions(this Tween tween, System.Action[] actions)
    {
        if (actions == null || actions.Length == 0)
        {
            return tween;
        }
        return tween.OnComplete(() =>
        {
            foreach (var action in actions)
            {
                action?.Invoke();
            }
        });
    }

    public static void Slide(this RectTransform target, float slideTime, float scalarSlideAnimation, params System.Action[] onCompleteActions)
    {
        target.DOAnchorPosX(target.anchoredPosition.x + scalarSlideAnimation, slideTime)
            .AddOnCompleteActions(onCompleteActions);
    }

    public static void SlideIn(this RectTransform left, RectTransform right, float slideTime, float scalarSlideAnimation, params System.Action[] onCompleteActions)
    {
        left.DOAnchorPosX(left.anchoredPosition.x + scalarSlideAnimation, slideTime);
        right.DOAnchorPosX(right.anchoredPosition.x - scalarSlideAnimation, slideTime)
            .AddOnCompleteActions(onCompleteActions);
    }

    public static void SlideOut(this RectTransform left, RectTransform right, float slideTime, float scalarSlideAnimation, params System.Action[] onCompleteActions)
    {
        left.DOAnchorPosX(left.anchoredPosition.x - scalarSlideAnimation, slideTime).SetEase(Ease.OutQuad);
        right.DOAnchorPosX(right.anchoredPosition.x + scalarSlideAnimation, slideTime).SetEase(Ease.OutQuad)
            .AddOnCompleteActions(onCompleteActions);
    }

    public static void Fade(this Graphic graphic, float targetFadeValue, float fadeTime, params System.Action[] onCompleteActions)
    {
        graphic.DOFade(targetFadeValue, fadeTime).SetEase(Ease.OutQuad)
            .AddOnCompleteActions(onCompleteActions);
    }

    public static void Fade(this CanvasGroup canvasGroup, float targetFadeValue, float fadeTime, params System.Action[] onCompleteActions)
    {
        canvasGroup.DOFade(targetFadeValue, fadeTime).SetEase(Ease.OutQuad)
            .AddOnCompleteActions(onCompleteActions);
    }

    public static void PopIn(this RectTransform target, float time, params System.Action[] onCompleteActions)
    {
        target.localScale = Vector3.zero;
        target.DOScale(Vector3.one, time).SetEase(Ease.OutBack)
            .AddOnCompleteActions(onCompleteActions);
    }

    public static void PopOut(this RectTransform target, float time, params System.Action[] onCompleteActions)
    {
        target.DOScale(Vector3.zero, time).SetEase(Ease.InBack)
            .AddOnCompleteActions(onCompleteActions);
    }

    public static void PunchScale(this RectTransform target, float duration = 0.2f, params System.Action[] onCompleteActions)
    {
        target.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), duration, 1, 1f)
            .AddOnCompleteActions(onCompleteActions);
    }

    public static void Shake(this RectTransform target, float duration = 0.5f, params System.Action[] onCompleteActions)
    {
        target.DOShakeAnchorPos(duration, new Vector2(10f, 0f), 10, 90f, false, true)
            .AddOnCompleteActions(onCompleteActions);
    }

    public static void Blink(this Graphic target, float targetFadeValue = 0f, float duration = 0.5f, int loops = -1, params System.Action[] onCompleteActions)
    {
        target.DOFade(targetFadeValue, duration).SetLoops(loops, LoopType.Yoyo)
            .AddOnCompleteActions(onCompleteActions);
    }

    public static void Blink(this CanvasGroup target, float targetFadeValue = 0f, float duration = 0.5f, int loops = -1, params System.Action[] onCompleteActions)
    {
        target.DOFade(targetFadeValue, duration).SetLoops(loops, LoopType.Yoyo)
            .AddOnCompleteActions(onCompleteActions);
    }
}