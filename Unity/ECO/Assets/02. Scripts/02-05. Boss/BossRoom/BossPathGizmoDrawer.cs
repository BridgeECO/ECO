using System.Collections.Generic;
using UnityEngine;

public static class BossPathGizmoDrawer
{
    public static void DrawSection(
        IReadOnlyList<Vector3> keyPoints,
        IReadOnlyList<Vector3> tangents,
        int sectionIndex,
        int resolution,
        float splineTension)
    {
        Vector3 startPosition = keyPoints[sectionIndex];
        Vector3 endPosition = keyPoints[sectionIndex + 1];
        float distance = Vector3.Distance(startPosition, endPosition);
        Vector3 startTangent = tangents[sectionIndex] * (distance * splineTension);
        Vector3 endTangent = tangents[sectionIndex + 1] * (distance * splineTension);
        Vector3 previousPosition = startPosition;
        Gizmos.color = Color.cyan;

        for (int i = 1; i <= resolution; i++)
        {
            float interpolation = i / (float)resolution;
            Vector3 nextPosition = SplineUtility.GetHermiteCurvePosition(
                interpolation,
                startPosition,
                endPosition,
                startTangent,
                endTangent);

            Gizmos.DrawLine(previousPosition, nextPosition);
            previousPosition = nextPosition;
        }
    }
}
