using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>크기를 랜덤하게 떨리게 한다. 반복 + Yoyo가 만드는 규칙적인 왕복과 달리 매 진동마다 방향이 바뀐다.</summary>
[Serializable]
[Preserve]
public class UI_ScaleShakeReaction : UI_ShakeReactionBase
{
    [SerializeField]
    [Tooltip("떨림의 진폭입니다. 배율이 아니라 기준 크기에 더해지는 절대량입니다.")]
    private Vector3 _strength = new Vector3(0.1f, 0.1f, 0f);

    public override EUIReactionChannel Channel => EUIReactionChannel.Scale;

    protected override void ApplyStartValue(UI_ReactionContext context, GameObject target)
    {
        Vector3 baseline = EnsureBaseline(context, target, target.transform.localScale);

        if (IsStartFromBaseline)
        {
            target.transform.localScale = baseline;
        }
    }

    protected override Tween CreatePlayTween(UI_ReactionContext context, GameObject target)
    {
        EnsureBaseline(context, target, target.transform.localScale);
        return target.transform.DOShakeScale(Motion.Duration, _strength, Vibrato, Randomness, IsFadeOut);
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
}
