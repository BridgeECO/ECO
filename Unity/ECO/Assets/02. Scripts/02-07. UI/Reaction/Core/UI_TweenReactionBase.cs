using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// DOTween으로 값을 굴리는 리액션의 공통 뼈대. 중단·복귀 정책을 여기서 한 번만 처리하고,
/// 파생 클래스는 어떤 값을 어떻게 굴릴지(트윈 생성)와 기준값 읽기·쓰기만 채운다.
/// </summary>
[Serializable]
public abstract class UI_TweenReactionBase : UI_ReactionBase
{
    [SerializeField]
    private UI_MotionSettings _motion = new UI_MotionSettings();

    [SerializeField]
    [Tooltip("체크하면 재생 직전에 시작값으로 먼저 옮긴 뒤 목표값으로 굴립니다. 팝업 개폐처럼 항상 같은 지점에서 출발해야 하는 연출에 씁니다.")]
    private bool _isUseStartValue = false;

    private UI_TweenPolicyRunner _runner;

    public override bool IsPlaying => Runner.IsPlaying;

    protected UI_MotionSettings Motion => _motion;

    protected bool IsUseStartValue => _isUseStartValue;

    // [SerializeReference] 역직렬화 경로에서 초기화가 도는지에 기대지 않고 지연 생성한다.
    protected UI_TweenPolicyRunner Runner
    {
        get
        {
            if (_runner == null)
            {
                _runner = new UI_TweenPolicyRunner();
            }

            return _runner;
        }
    }

    // async를 붙이지 않는다. hover가 오갈 때마다 지나가는 경로라 상태머신 객체가 그대로 쓰레기가 된다.
    // 기다림이 필요한 드문 정책만 아래 async 헬퍼로 빠진다.
    public override UniTask PlayAsync(UI_ReactionContext context,
        EUIReactionInterruptPolicy interruptPolicy, CancellationToken cancellationToken)
    {
        GameObject target = ResolveTarget(context);
        if (target == null || !TryEnterPlay(interruptPolicy))
        {
            return UniTask.CompletedTask;
        }

        if (interruptPolicy == EUIReactionInterruptPolicy.PlayToEndThenPlay && Runner.IsPlaying)
        {
            return PlayAfterCurrentAsync(context, target, cancellationToken);
        }

        return StartPlayTween(context, target);
    }

    public override UniTask ExitAsync(UI_ReactionContext context,
        EUIReactionExitPolicy exitPolicy, CancellationToken cancellationToken)
    {
        if (exitPolicy == EUIReactionExitPolicy.Keep)
        {
            return UniTask.CompletedTask;
        }

        GameObject target = ResolveTarget(context);
        if (target == null)
        {
            return UniTask.CompletedTask;
        }

        if (exitPolicy == EUIReactionExitPolicy.Reverse && Runner.TryReverse())
        {
            return Runner.WaitAsync();
        }

        if (exitPolicy == EUIReactionExitPolicy.PlayToEnd)
        {
            return RestoreAfterCurrentAsync(context, cancellationToken);
        }

        if (exitPolicy == EUIReactionExitPolicy.TweenToBaseline)
        {
            Runner.Run(_motion.ApplyTo(CreateExitTween(context, target)), target);
            return Runner.WaitAsync();
        }

        RestoreBaseline(context);
        return UniTask.CompletedTask;
    }

    private UniTask StartPlayTween(UI_ReactionContext context, GameObject target)
    {
        ApplyStartValue(context, target);
        Runner.Run(_motion.ApplyTo(CreatePlayTween(context, target)), target);

        // 무한 반복은 끝나지 않으므로 기다리면 호출부가 영영 풀리지 않는다.
        return _motion.IsInfiniteLoop ? UniTask.CompletedTask : Runner.WaitAsync();
    }

    private async UniTask PlayAfterCurrentAsync(UI_ReactionContext context, GameObject target,
        CancellationToken cancellationToken)
    {
        await Runner.WaitAsync();
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        await StartPlayTween(context, target);
    }

    private async UniTask RestoreAfterCurrentAsync(UI_ReactionContext context, CancellationToken cancellationToken)
    {
        await Runner.WaitAsync();
        if (!cancellationToken.IsCancellationRequested)
        {
            RestoreBaseline(context);
        }
    }

    public override void Kill()
    {
        Runner.Kill(false);
    }

    public override void RestoreBaseline(UI_ReactionContext context)
    {
        Runner.Kill(false);

        GameObject target = ResolveTarget(context);
        if (target == null)
        {
            return;
        }

        ApplyBaseline(context, target);
    }

    protected Vector4 EnsureBaseline(UI_ReactionContext context, GameObject target, Vector4 currentValue)
    {
        return EnsureBaseline(context, target, currentValue, null, false).Value;
    }

    protected bool TryGetBaseline(UI_ReactionContext context, GameObject target, out Vector4 value)
    {
        if (TryGetBaseline(context, target, out UI_ReactionBaseline stored))
        {
            value = stored.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// 재생 직전에 출발 지점을 맞춘다. 시작값을 쓰지 않고 기준값에서 출발하는 모드도 여기서 처리한다.
    /// </summary>
    protected abstract void ApplyStartValue(UI_ReactionContext context, GameObject target);

    protected abstract Tween CreatePlayTween(UI_ReactionContext context, GameObject target);

    protected abstract Tween CreateExitTween(UI_ReactionContext context, GameObject target);

    protected abstract void ApplyBaseline(UI_ReactionContext context, GameObject target);

    // 정책상 새 재생을 시작해도 되는지. 대기가 필요한 정책은 호출부에서 따로 처리한다.
    private bool TryEnterPlay(EUIReactionInterruptPolicy interruptPolicy)
    {
        switch (interruptPolicy)
        {
            case EUIReactionInterruptPolicy.IgnoreUntilDone:
            case EUIReactionInterruptPolicy.SkipIfSame:
                return !Runner.IsPlaying;

            // 재생 중일 때만 되감는다. 완료된 트윈은 AutoKill을 꺼 둔 탓에 살아 있을 뿐이라,
            // 이걸 되감으면 새로 요청한 재생이 거꾸로 도는 꼴이 된다.
            case EUIReactionInterruptPolicy.Reverse:
                if (Runner.IsPlaying)
                {
                    Runner.TryReverse();
                    return false;
                }

                return true;

            default:
                return true;
        }
    }
}
