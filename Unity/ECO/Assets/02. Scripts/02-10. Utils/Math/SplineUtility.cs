using System.Collections.Generic;
using UnityEngine;

public static class SplineUtility
{
    public static Vector3[] CalculateTangents(List<Vector3> points)
    {
        Vector3[] tangents = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 pPrev = i > 0 ? points[i - 1] : points[i];
            Vector3 pCurr = points[i];
            Vector3 pNext = i < points.Count - 1 ? points[i + 1] : points[i];

            Vector3 dIn = (pCurr - pPrev).normalized;
            Vector3 dOut = (pNext - pCurr).normalized;

            if (i == 0)
            {
                dIn = dOut;
            }

            if (i == points.Count - 1)
            {
                dOut = dIn;
            }

            Vector3 dAvg = (dIn + dOut).normalized;
            if (dAvg.sqrMagnitude < 0.01f)
            {
                dAvg = dOut;
            }

            tangents[i] = dAvg;
        }

        return tangents;
    }

    public static Vector3 GetHermiteCurvePosition(float t, Vector3 p0, Vector3 p1, Vector3 m0, Vector3 m1)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        float h00 = 2f * t3 - 3f * t2 + 1f;
        float h10 = t3 - 2f * t2 + t;
        float h01 = -2f * t3 + 3f * t2;
        float h11 = t3 - t2;

        return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
    }
}
