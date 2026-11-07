#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 코어를 선택하면 스냅포인트마다, 그 스냅포인트가 놓인 지형 경로 위에 안착 범위를 굵게 그린다.
/// 범위는 경로를 따라 잰 거리라 꺾인 경로에서는 직선 반경과 다르므로, 경로 위에서 바로 확인하게 한다.
/// </summary>
public static class SnapRangeGizmoDrawer
{
    private const float RANGE_THICKNESS = 4f;
    private const float PATH_POINT_RADIUS = 0.08f;

    public static void Draw(IReadOnlyList<TerrainObject> terrains, IReadOnlyList<SnapPoint> snapPoints)
    {
        TerrainPathMeasure measure = new TerrainPathMeasure();
        Handles.color = Color.magenta;
        Gizmos.color = Color.magenta;

        for (int i = 0; i < terrains.Count; i++)
        {
            if (!SnapPathEditorUtility.TryInitMeasure(terrains[i], measure))
            {
                continue;
            }

            for (int j = 0; j < snapPoints.Count; j++)
            {
                SnapPoint snapPoint = snapPoints[j];
                if (snapPoint == null ||
                    !measure.TryFindDistance(snapPoint.transform.position, SnapPoint.PATH_TOLERANCE, out float distance))
                {
                    continue;
                }

                DrawRange(measure, distance - snapPoint.SnapDistance, distance + snapPoint.SnapDistance);
                Gizmos.DrawSphere(measure.GetPosition(distance), PATH_POINT_RADIUS);
            }
        }
    }

    private static void DrawRange(TerrainPathMeasure measure, float from, float to)
    {
        from = Mathf.Max(0f, from);
        to = Mathf.Min(measure.TotalLength, to);

        Vector2 previous = measure.GetPosition(from);
        float accumulated = 0f;
        for (int i = 1; i < measure.VertexCount; i++)
        {
            accumulated += Vector2.Distance(measure.GetVertex(i - 1), measure.GetVertex(i));
            if (accumulated <= from)
            {
                continue;
            }

            if (to <= accumulated)
            {
                break;
            }

            Vector2 vertex = measure.GetVertex(i);
            Handles.DrawLine(previous, vertex, RANGE_THICKNESS);
            previous = vertex;
        }
        Handles.DrawLine(previous, measure.GetPosition(to), RANGE_THICKNESS);
    }
}
#endif
