#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// 선과 분기의 곡선, 분기점, 코어의 켜짐·꺼짐 판정 지점을 씬 뷰에 그린다.
/// </summary>
public static class EnergyLineGizmoDrawer
{
    private const float FORK_RADIUS = 0.15f;
    private const float JUDGE_POINT_RADIUS = 0.1f;

    public static void Draw(EnergyLineGraph graph)
    {
        for (int i = 0; i < graph.Paths.Count; i++)
        {
            EnergyLineBranchPath path = graph.Paths[i];
            DrawCurve(path);
            DrawForkPoint(path);
            DrawJudgePoints(path);
        }
    }

    private static void DrawCurve(EnergyLineBranchPath path)
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < path.Waypoints.Count - 1; i++)
        {
            Gizmos.DrawLine(path.Waypoints[i], path.Waypoints[i + 1]);
        }
    }

    private static void DrawForkPoint(EnergyLineBranchPath path)
    {
        if (path.ParentIndex < 0)
        {
            return;
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(path.SamplePosition(0f), FORK_RADIUS);
    }

    // 초록은 에너지구가 닿으면 코어가 켜지는 중심, 빨강은 에너지가 완전히 빠져나가야 꺼지는 출구다.
    private static void DrawJudgePoints(EnergyLineBranchPath path)
    {
        for (int i = 0; i < path.Nodes.Count; i++)
        {
            EnergyPathNode node = path.Nodes[i];
            if (node.NodeType != EEnergyPathNodeType.EnergyCore || node.EnergyCore == null)
            {
                continue;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(path.SamplePosition(node.ActivationCenterDistance - path.StartDistance), JUDGE_POINT_RADIUS);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(path.SamplePosition(node.DeactivationEndDistance - path.StartDistance), JUDGE_POINT_RADIUS);
        }
    }
}
#endif
