using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>정해진 시간만 흘려보낸다.</summary>
[Serializable]
[Preserve]
public class CutsceneWaitStep : CutsceneStepBase
{
    [SerializeField]
    [Min(0f)]
    private float _duration = 1f;

    [SerializeField]
    [Tooltip("일시정지 중에도 시간이 흐르게 합니다.")]
    private bool _isIgnoreTimeScale = false;

    public override async UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        if (_duration <= 0f)
        {
            return;
        }

        DelayType delayType = _isIgnoreTimeScale ? DelayType.UnscaledDeltaTime : DelayType.DeltaTime;
        await UniTask.Delay(TimeSpan.FromSeconds(_duration), delayType, PlayerLoopTiming.Update, cancellationToken);
    }

    // 지나간 대기를 몰아서 기다릴 이유가 없다.
    public override void ApplyFinalState(CutsceneContext context)
    {
    }
}
