using System.Collections.Generic;
using UnityEngine;

public class EnergyLineRendererDrawer
{
    // 펄스가 움직이는 동안 매 프레임 호출되므로 점 목록을 새로 만들지 않고 재사용한다.
    private List<Vector3> _points = new List<Vector3>();

    public void UpdateSegmentRenderer(LineRenderer lineRenderer, EnergyLineBranchPath path, float startDist, float endDist)
    {
        IReadOnlyList<Vector3> waypoints = path.Waypoints;
        IReadOnlyList<float> distances = path.CumulativeDistances;
        if (waypoints.Count < 2)
        {
            return;
        }

        _points.Clear();
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            float currentDist = distances[i];
            float nextDist = distances[i + 1];
            if (nextDist < startDist || endDist < currentDist)
            {
                continue;
            }

            float segLen = nextDist - currentDist;
            float tStart = 0f < segLen ? Mathf.Max(0f, startDist - currentDist) / segLen : 0f;
            float tEnd = 0f < segLen ? Mathf.Min(segLen, endDist - currentDist) / segLen : 1f;

            Vector3 pointStart = Vector3.Lerp(waypoints[i], waypoints[i + 1], tStart);
            Vector3 pointEnd = Vector3.Lerp(waypoints[i], waypoints[i + 1], tEnd);

            if (_points.Count == 0 || _points[_points.Count - 1] != pointStart)
            {
                _points.Add(pointStart);
            }
            _points.Add(pointEnd);
        }

        lineRenderer.positionCount = _points.Count;
        for (int i = 0; i < _points.Count; i++)
        {
            lineRenderer.SetPosition(i, _points[i]);
        }
    }
}
