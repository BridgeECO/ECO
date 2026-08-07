using DG.Tweening;
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

    public static void Blink(this Graphic target, float targetFadeValue = 0f, float duration = 0.5f, int loops = -1, params System.Action[] onCompleteActions)
    {
        target.DOFade(targetFadeValue, duration).SetLoops(loops, LoopType.Yoyo)
            .AddOnCompleteActions(onCompleteActions);
    }
}
