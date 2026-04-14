using System.Collections.Generic;
using UnityEngine;

public class EnergyLineRendererDrawer
{
    public void UpdateSegmentRenderer(LineRenderer lineRenderer, List<Vector3> computedWaypoints, float startDist, float endDist)
    {
        if (computedWaypoints == null || computedWaypoints.Count < 2)
        {
            return;
        }

        List<Vector3> points = new List<Vector3>();
        float currentDist = 0f;

        for (int i = 0; i < computedWaypoints.Count - 1; i++)
        {
            Vector3 p1 = computedWaypoints[i];
            Vector3 p2 = computedWaypoints[i + 1];
            float segLen = Vector3.Distance(p1, p2);
            float nextDist = currentDist + segLen;

            if (startDist <= nextDist && endDist >= currentDist)
            {
                float localStart = Mathf.Max(0, startDist - currentDist);
                float localEnd = Mathf.Min(segLen, endDist - currentDist);

                Vector3 pointStart = Vector3.Lerp(p1, p2, localStart / segLen);
                Vector3 pointEnd = Vector3.Lerp(p1, p2, localEnd / segLen);

                if (points.Count == 0 || points[points.Count - 1] != pointStart)
                {
                    points.Add(pointStart);
                }

                points.Add(pointEnd);
            }

            currentDist = nextDist;
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}
