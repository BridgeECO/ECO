using DG.Tweening;
using UnityEngine;

/// <summary>
/// 중단·복귀 정책이 트윈 리액션에게 요구하는 것.
/// 정책 처리를 리액션 밖으로 빼면서, 델리게이트를 넘기는 대신 이 계약으로 호출한다.
/// hover가 오갈 때마다 지나가는 경로라 클로저가 잡히면 그대로 쓰레기가 된다.
/// </summary>
public interface IUITweenReaction
{
    UI_TweenPolicyRunner Runner { get; }

    /// <summary>무한 반복은 끝나지 않으므로 정책이 완료를 기다리면 안 된다.</summary>
    bool IsInfiniteLoop { get; }

    GameObject ResolveTarget(UI_ReactionContext context);

    void ApplyStartValue(UI_ReactionContext context, GameObject target);

    Tween CreatePlayTween(UI_ReactionContext context, GameObject target);

    Tween CreateExitTween(UI_ReactionContext context, GameObject target);

    /// <summary>모션 설정을 입힌다. 정책이 UI_MotionSettings를 몰라도 되도록 여기서 가린다.</summary>
    Tween ApplyPlayMotion(Tween tween);

    Tween ApplyExitMotion(Tween tween);

    void RestoreBaseline(UI_ReactionContext context);
}
