using System.Collections.Generic;
using UnityEngine;

public class EnergyPathCalculator
{
    public void CalculatePathAndDistances(Transform start, Transform end, Transform defaultTransform, List<EnergyPathNode> pathNodes, int resolution, float tension, out List<Vector3> computedWaypoints, out float totalDistance)
    {
        List<Vector3> keyPoints = GetKeyPoints(start, end, defaultTransform, pathNodes);
        computedWaypoints = new List<Vector3>();
        totalDistance = 0f;

        if (keyPoints.Count < 2)
        {
            return;
        }

        Vector3[] tangents = SplineUtility.CalculateTangents(keyPoints);
        Dictionary<int, float> keyPointDistances = new Dictionary<int, float>();
        keyPointDistances[0] = 0f;
        computedWaypoints.Add(keyPoints[0]);

        for (int i = 0; i < keyPoints.Count - 1; i++)
        {
            Vector3 p0 = keyPoints[i];
            Vector3 p1 = keyPoints[i + 1];
            float dist = Vector3.Distance(p0, p1);

            Vector3 m0 = tangents[i] * (dist * tension);
            Vector3 m1 = tangents[i + 1] * (dist * tension);

            for (int j = 1; j <= resolution; j++)
            {
                float t = j / (float)resolution;
                Vector3 point = SplineUtility.GetHermiteCurvePosition(t, p0, p1, m0, m1);

                float segDist = Vector3.Distance(computedWaypoints[computedWaypoints.Count - 1], point);
                totalDistance += segDist;
                computedWaypoints.Add(point);

                if (j == resolution)
                {
                    keyPointDistances[i + 1] = totalDistance;
                }
            }
        }

        int keyIndex = 1;
        for (int i = 0; i < pathNodes.Count; i++)
        {
            EnergyPathNode node = pathNodes[i];

            if (node.NodeType == EEnergyPathNodeType.Terrain)
            {
                if (node.Terrain == null)
                {
                    continue;
                }
                int actIndex = keyIndex;
                int deactIndex = keyIndex + 1;
                node.ActivationCenterDistance = keyPointDistances.ContainsKey(actIndex) ? keyPointDistances[actIndex] : 0f;
                node.DeactivationEndDistance = keyPointDistances.ContainsKey(deactIndex) ? keyPointDistances[deactIndex] : 0f;
                keyIndex += 2;
            }
            else
            {
                if (node.Waypoint == null)
                {
                    continue;
                }
                keyIndex += 1;
            }
        }
    }

    public List<Vector3> GetKeyPoints(Transform start, Transform end, Transform defaultTransform, List<EnergyPathNode> pathNodes)
    {
        List<Vector3> points = new List<Vector3>();
        AddKeyPoint(points, start, defaultTransform);

        foreach (EnergyPathNode node in pathNodes)
        {
            if (node.NodeType == EEnergyPathNodeType.Terrain)
            {
                if (node.Terrain == null)
                {
                    continue;
                }

                if (node.IsCaptured)
                {
                    points.Add(node.StaticActivationPosition);
                    points.Add(node.StaticDeactivationPosition);
                }
                else
                {
                    AddKeyPoint(points, node.Terrain.ActivationPosition, node.Terrain.transform);
                    AddKeyPoint(points, node.Terrain.DeactivationPosition, node.Terrain.transform);
                }
            }
            else
            {
                if (node.Waypoint == null)
                {
                    continue;
                }
                points.Add(node.Waypoint.position);
            }
        }

        AddKeyPoint(points, end, defaultTransform);
        return points;
    }

    private static void AddKeyPoint(List<Vector3> targetList, Transform targetTransform, Transform fallbackTransform)
    {
        if (targetTransform != null)
        {
            targetList.Add(targetTransform.position);
            return;
        }
        targetList.Add(fallbackTransform.position);
    }
}
