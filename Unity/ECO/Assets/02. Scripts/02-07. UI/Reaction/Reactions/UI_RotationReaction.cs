using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// 대상을 회전시킨다. localEulerAngles는 Layout이 덮어쓰지 않아 어디에나 안전하다.
/// </summary>
[Serializable]
[Preserve]
public class UI_RotationReaction : UI_TweenReactionBase
{
    [SerializeField]
    [Tooltip("기준값 기준 모드에서는 회전량으로, 절대값 모드에서는 목표 각도로 쓰입니다.")]
    private Vector3 _euler = new Vector3(0f, 0f, 10f);

    [SerializeField]
    private Vector3 _startEuler = Vector3.zero;

    public override EUIReactionChannel Channel => EUIReactionChannel.Rotation;

    protected override void ApplyStartValue(UI_ReactionContext context, GameObject target)
    {
        Vector3 baseline = EnsureBaseline(context, target, target.transform.localEulerAngles);

        if (IsUseStartValue)
        {
            target.transform.localEulerAngles = ResolveValue(_startEuler, baseline);
            return;
        }

        if (Motion.ValueMode == EUIReactionValueMode.FromBaseline)
        {
            target.transform.localEulerAngles = baseline;
        }
    }

    protected override Tween CreatePlayTween(UI_ReactionContext context, GameObject target)
    {
        Vector3 baseline = EnsureBaseline(context, target, target.transform.localEulerAngles);

        // 360도를 넘는 회전도 지정한 만큼 그대로 돌도록 FastBeyond360을 쓴다.
        return target.transform.DOLocalRotate(ResolveValue(_euler, baseline), Motion.Duration,
            RotateMode.FastBeyond360);
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

    private Vector3 ResolveValue(Vector3 value, Vector3 baseline)
    {
        if (Motion.ValueMode == EUIReactionValueMode.RelativeToBaseline)
        {
            return baseline + value;
        }

        return value;
    }
}
