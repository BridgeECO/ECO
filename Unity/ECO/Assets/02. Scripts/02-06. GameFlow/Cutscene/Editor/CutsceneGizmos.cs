using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 선택한 컷씬이 카메라를 어디로 보내는지 씬 뷰에 그린다.
/// 카메라 지점은 대개 빈 오브젝트라 그리지 않으면 어디 있는지 보이지 않는다.
///
/// [DrawGizmo]로 붙이므로 CutsceneDirector에는 한 줄도 들어가지 않는다.
/// </summary>
public static class CutsceneGizmos
{
    private const float POINT_RADIUS = 0.35f;

    private static readonly Color _pointColor = new Color(0.3f, 0.8f, 1f, 0.9f);

    [DrawGizmo(GizmoType.Selected | GizmoType.InSelectionHierarchy)]
    private static void DrawDirector(CutsceneDirector director, GizmoType gizmoType)
    {
        DrawSteps(director, director.Steps, 1);
    }

    private static void DrawSteps(CutsceneDirector director, IReadOnlyList<CutsceneStepBase> steps, int depth)
    {
        if (steps == null || CutsceneGroupStepBase.MAX_NEST_DEPTH < depth)
        {
            return;
        }

        for (int i = 0; i < steps.Count; i++)
        {
            CutsceneStepBase step = steps[i];
            if (ReferenceEquals(step, null))
            {
                continue;
            }

            if (step is CutsceneGroupStepBase group)
            {
                DrawSteps(director, group.Steps, depth + 1);
                continue;
            }

            if (step is CutsceneCameraMoveStep cameraStep && cameraStep.ViewPoint != null)
            {
                DrawPoint(director, cameraStep.ViewPoint, i + 1);
            }
        }
    }

    private static void DrawPoint(CutsceneDirector director, Transform viewPoint, int stepNumber)
    {
        Color previous = Gizmos.color;
        Gizmos.color = _pointColor;

        Gizmos.DrawWireSphere(viewPoint.position, POINT_RADIUS);
        Gizmos.DrawLine(director.transform.position, viewPoint.position);

        Gizmos.color = previous;
        Handles.Label(viewPoint.position + Vector3.up * POINT_RADIUS, stepNumber.ToString("00") + " 카메라");
    }
}
