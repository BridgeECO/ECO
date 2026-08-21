using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>회전을 랜덤하게 떨리게 한다. localEulerAngles는 레이아웃의 영향을 받지 않아 어디에나 안전하다.</summary>
[Serializable]
[Preserve]
public class UI_RotationShakeReaction : UI_ShakeReactionBase
{
    [SerializeField]
    [Tooltip("떨림의 진폭입니다(도). 기준 각도에 더해지는 절대량입니다.")]
    private Vector3 _strength = new Vector3(0f, 0f, 10f);

    public override EUIReactionChannel Channel => EUIReactionChannel.Rotation;

    protected override void ApplyStartValue(UI_ReactionContext context, GameObject target)
    {
        Vector3 baseline = EnsureBaseline(context, target, target.transform.localEulerAngles);

        if (IsStartFromBaseline)
        {
            target.transform.localEulerAngles = baseline;
        }
    }

    protected override Tween CreatePlayTween(UI_ReactionContext context, GameObject target)
    {
        EnsureBaseline(context, target, target.transform.localEulerAngles);
        return target.transform.DOShakeRotation(Motion.Duration, _strength, Vibrato, Randomness, IsFadeOut);
    }

    protected override Tween CreateExitTween(UI_ReactionContext context, GameObject target)
    {
        if (!TryGetBaseline(context, target, out Vector4 baseline))
        {
            return null;
        }

        return target.transform.DOLocalRotate((Vector3)baseline, Motion.Duration, RotateMode.FastBeyond360);
    }

    protected override void ApplyBaseline(UI_ReactionContext context, GameObject target)
    {
        if (!TryGetBaseline(context, target, out Vector4 baseline))
        {
            return;
        }

        target.transform.localEulerAngles = baseline;
    }
}
