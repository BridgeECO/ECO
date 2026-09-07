using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using VInspector;

/// <summary>플레이어를 제자리에 세우거나 조작을 돌려준다.</summary>
[Serializable]
[Preserve]
public class CutscenePlayerControlStep : CutsceneStepBase
{
    [SerializeField]
    [Tooltip("체크하면 조작을 막고 제자리에 세웁니다. 해제하면 조작을 돌려줍니다.")]
    private bool _isControlBlocked = true;

    [ShowIf(nameof(_isControlBlocked))]
    [SerializeField]
    [Tooltip("공중이면 착지할 때까지 기다린 뒤 세웁니다. 허공에 뜬 채 얼어붙는 것을 막습니다.")]
    private bool _isWaitForGrounded = true;

    [ShowIf(nameof(_isControlBlocked))]
    [SerializeField]
    [Min(0f)]
    [Tooltip("착지를 이만큼 기다려도 땅에 닿지 않으면 그냥 진행합니다.")]
    private float _groundWaitTimeout = 3f;
    [EndIf]

    public bool IsControlBlocked => _isControlBlocked;

    public override async UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        if (!_isControlBlocked)
        {
            ReleaseControl(context);
            return;
        }

        context.InputLease.Acquire();
        await context.ActorFreeze.FreezeAsync(context, _isWaitForGrounded, _groundWaitTimeout, cancellationToken);
    }

    public override void ApplyFinalState(CutsceneContext context)
    {
        if (!_isControlBlocked)
        {
            ReleaseControl(context);
            return;
        }

        context.InputLease.Acquire();
        context.ActorFreeze.Freeze(context);
    }

    private static void ReleaseControl(CutsceneContext context)
    {
        // 세울 때와 반대 순서다. 플레이어를 먼저 풀어야 입력이 살아나는 프레임에 이미 움직일 수 있다.
        context.ActorFreeze.Release();
        context.InputLease.Release();
    }
}
