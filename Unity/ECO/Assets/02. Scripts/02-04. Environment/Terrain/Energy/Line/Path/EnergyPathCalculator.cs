using System.Collections.Generic;
using UnityEngine;

public class EnergyPathCalculator
{
    private const float COINCIDENT_EXIT_EPSILON = 0.001f;

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
        Dictionary<int, float> keyPointDistances = ComputeSpline(keyPoints, tangents, resolution, tension, computedWaypoints, ref totalDistance);
        AssignNodeDistances(pathNodes, keyPointDistances);
    }

    public List<Vector3> GetKeyPoints(Transform start, Transform end, Transform defaultTransform, List<EnergyPathNode> pathNodes)
    {
        List<Vector3> points = new List<Vector3>();
        AddKeyPoint(points, start, defaultTransform);

        foreach (EnergyPathNode node in pathNodes)
        {
            AddNodeKeyPoints(points, node);
        }

        AddKeyPoint(points, end, defaultTransform);
        return points;
    }

    private void AddKeyPoint(List<Vector3> targetList, Transform targetTransform, Transform fallbackTransform)
    {
        targetList.Add(GetPosition(targetTransform, fallbackTransform));
    }

    private Vector3 GetPosition(Transform targetTransform, Transform fallbackTransform)
    {
        return targetTransform != null ? targetTransform.position : fallbackTransform.position;
    }

    private void AddNodeKeyPoints(List<Vector3> points, EnergyPathNode node)
    {
        switch (node.NodeType)
        {
            case EEnergyPathNodeType.Waypoint:
                AddWaypointKeyPoints(points, node);
                break;
            case EEnergyPathNodeType.EnergyCore:
                AddEnergyCoreKeyPoints(points, node);
                break;
        }
    }

    private void AddWaypointKeyPoints(List<Vector3> points, EnergyPathNode node)
    {
        if (node.Waypoint == null)
        {
            return;
        }
        points.Add(node.Waypoint.position);
    }

    private void AddEnergyCoreKeyPoints(List<Vector3> points, EnergyPathNode node)
    {
        if (node.EnergyCore == null)
        {
            return;
        }
        AddKeyPoint(points, node.EnergyCore.ActivationPosition, node.EnergyCore.transform);
        if (HasSeparateExit(node.EnergyCore))
        {
            AddKeyPoint(points, node.EnergyCore.DeactivationPosition, node.EnergyCore.transform);
        }
    }

    // 입구와 출구가 같은 자리인 코어(P_EnergyCore 기본값)를 키포인트 두 개로 넣으면 길이 0인 구간이 생긴다.
    // 그 구간 양 끝의 접선이 들어오는 방향과 나가는 방향으로 갈라져 곡선이 코어에서 꺾이므로, 이때는 한 점만 넣는다.
    private bool HasSeparateExit(EnergyCore core)
    {
        Vector3 entry = GetPosition(core.ActivationPosition, core.transform);
        Vector3 exit = GetPosition(core.DeactivationPosition, core.transform);
        return COINCIDENT_EXIT_EPSILON * COINCIDENT_EXIT_EPSILON < (exit - entry).sqrMagnitude;
    }

    private Dictionary<int, float> ComputeSpline(List<Vector3> keyPoints, Vector3[] tangents, int resolution, float tension, List<Vector3> computedWaypoints, ref float totalDistance)
    {
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
                totalDistance += Vector3.Distance(computedWaypoints[computedWaypoints.Count - 1], point);
                computedWaypoints.Add(point);

                if (j == resolution)
                {
                    keyPointDistances[i + 1] = totalDistance;
                }
            }
        }
        return keyPointDistances;
    }

    private void AssignNodeDistances(List<EnergyPathNode> pathNodes, Dictionary<int, float> keyPointDistances)
    {
        int keyIndex = 1;
        for (int i = 0; i < pathNodes.Count; i++)
        {
            keyIndex = AdvanceNodeKeyIndex(pathNodes[i], keyPointDistances, keyIndex);
        }
    }

    private int AdvanceNodeKeyIndex(EnergyPathNode node, Dictionary<int, float> keyPointDistances, int keyIndex)
    {
        if (node.NodeType == EEnergyPathNodeType.Waypoint)
        {
            return node.Waypoint != null ? keyIndex + 1 : keyIndex;
        }

        if (!HasValidDualPoint(node))
        {
            return keyIndex;
        }

        node.ActivationCenterDistance = GetKeyDistance(keyPointDistances, keyIndex);
        if (!HasSeparateExit(node.EnergyCore))
        {
            // 출구 거리는 EnergyLineGraph가 코어 반경만큼 뒤로 보정한다.
            node.DeactivationEndDistance = node.ActivationCenterDistance;
            return keyIndex + 1;
        }

        node.DeactivationEndDistance = GetKeyDistance(keyPointDistances, keyIndex + 1);
        return keyIndex + 2;
    }

    private bool HasValidDualPoint(EnergyPathNode node)
    {
        return node.NodeType == EEnergyPathNodeType.EnergyCore && node.EnergyCore != null;
    }

    private float GetKeyDistance(Dictionary<int, float> distances, int index)
    {
        return distances.ContainsKey(index) ? distances[index] : 0f;
    }
}