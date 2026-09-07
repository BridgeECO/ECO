using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 컷씬이 카메라를 옮긴다. 점유 중재는 CutsceneCameraLease가 따로 맡는다.
///
/// 움직이는 것은 CameraController가 붙은 루트다. 흔들기는 자식 MainCamera의 localPosition을
/// 쓰므로 채널이 달라, 동시 실행 그룹에 이동과 흔들기를 나란히 넣어도 서로를 덮지 않는다.
/// </summary>
public class CutsceneCameraMover
{
    /// <summary>
    /// 목표 위치는 반드시 GetClampedPosition을 통과시킨다. 이 메서드가 카메라의 현재 z를
    /// 보존하므로, 기획자가 z=0인 빈 오브젝트를 마커로 써도 카메라가 지형 평면에 박히지 않는다.
    /// 클램프를 끄는 선택지는 두지 않는다.
    /// </summary>
    public Vector3 ResolveTarget(CutsceneContext context, Transform viewPoint)
    {
        CameraController controller = context.CameraController;
        if (controller == null)
        {
            return Vector3.zero;
        }

        return viewPoint == null
            ? controller.GetClampedPosition()
            : controller.GetClampedPosition(viewPoint.position);
    }

    public async UniTask MoveToAsync(CutsceneContext context, Vector3 target, float duration, Ease ease,
        CancellationToken cancellationToken)
    {
        CameraController controller = context.CameraController;
        if (controller == null)
        {
            return;
        }

        if (duration <= 0f)
        {
            SnapTo(context, target);
            return;
        }

        await controller.PanToPositionAsync(target, duration, ease, cancellationToken);
    }

    public void SnapTo(CutsceneContext context, Vector3 target)
    {
        CameraController controller = context.CameraController;
        if (controller == null)
        {
            return;
        }

        controller.transform.position = target;
    }

    public UniTask ShakeAsync(CutsceneContext context, float duration, float strength, int vibrato,
        CancellationToken cancellationToken)
    {
        CameraEffect cameraEffect = context.CameraEffect;
        if (cameraEffect == null)
        {
            return UniTask.CompletedTask;
        }

        return cameraEffect.ShakeCameraAsync(duration, strength, vibrato, cancellationToken: cancellationToken);
    }
}
