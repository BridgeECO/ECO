using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>카메라를 특정 지점으로 옮긴다. 지점을 비우면 플레이어 쪽으로 돌아온다.</summary>
[Serializable]
[Preserve]
public class CutsceneCameraMoveStep : CutsceneStepBase
{
    [SerializeField]
    [Tooltip("비워 두면 플레이어 위치로 돌아옵니다.")]
    private Transform _viewPoint;

    [SerializeField]
    [Min(0f)]
    private float _duration = 1f;

    [SerializeField]
    private Ease _ease = Ease.InOutCubic;

    public Transform ViewPoint => _viewPoint;

    public override async UniTask PlayAsync(CutsceneContext context, CancellationToken cancellationToken)
    {
        if (!context.TryResolveCamera())
        {
            return;
        }

        // 첫 카메라 스텝이 제어권을 가져간다. 카메라를 건드리지 않는 컷씬은 빌리지도 않는다.
        context.CameraLease.Acquire(context);

        Vector3 target = context.CameraMover.ResolveTarget(context, _viewPoint);
        await context.CameraMover.MoveToAsync(context, target, _duration, _ease, cancellationToken);
    }

    public override void ApplyFinalState(CutsceneContext context)
    {
        if (!context.TryResolveCamera())
        {
            return;
        }

        // 리스를 잡지 않으면 같은 프레임에 FollowPlayer가 덮어써 스냅이 보이지 않는다.
        context.CameraLease.Acquire(context);
        context.CameraMover.SnapTo(context, context.CameraMover.ResolveTarget(context, _viewPoint));
    }
}
