using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 계산이 끝난 분기 하나의 곡선과 그 위의 노드.
/// 곡선 위 거리는 분기 시작점 기준이고, StartDistance를 더하면 에너지원 기준 거리가 된다.
/// </summary>
public class EnergyLineBranchPath
{
    private List<Vector3> _waypoints;
    private List<EnergyPathNode> _nodes;
    private List<float> _cumulativeDistances = new List<float>();

    public IReadOnlyList<Vector3> Waypoints => _waypoints;
    public IReadOnlyList<float> CumulativeDistances => _cumulativeDistances;
    public IReadOnlyList<EnergyPathNode> Nodes => _nodes;
    public float StartDistance { get; private set; }
    public float Length { get; private set; }
    public float EndDistance => StartDistance + Length;
    public int ParentIndex { get; private set; }

    public EnergyLineBranchPath(List<Vector3> waypoints, List<EnergyPathNode> nodes, float startDistance, int parentIndex)
    {
        _waypoints = waypoints;
        _nodes = nodes;
        StartDistance = startDistance;
        ParentIndex = parentIndex;
        InitCumulativeDistances();
    }

    public Vector3 SamplePosition(float localDistance)
    {
        if (_waypoints.Count == 0)
        {
            return Vector3.zero;
        }

        for (int i = 1; i < _waypoints.Count; i++)
        {
            if (localDistance <= _cumulativeDistances[i])
            {
                float segmentLength = _cumulativeDistances[i] - _cumulativeDistances[i - 1];
                float t = 0f < segmentLength ? (localDistance - _cumulativeDistances[i - 1]) / segmentLength : 0f;
                return Vector3.Lerp(_waypoints[i - 1], _waypoints[i], t);
            }
        }

        return _waypoints[_waypoints.Count - 1];
    }

    // 곡선은 키포인트를 정확히 지나므로, 분기점과 겹치는 꼭짓점의 누적 거리가 곧 분기 시작 거리다.
    public bool TryFindLocalDistance(Vector3 point, float tolerance, out float localDistance)
    {
        localDistance = 0f;
        float closestSqrDistance = tolerance * tolerance;
        bool isFound = false;

        for (int i = 0; i < _waypoints.Count; i++)
        {
            float sqrDistance = (_waypoints[i] - point).sqrMagnitude;
            if (closestSqrDistance < sqrDistance)
            {
                continue;
            }

            closestSqrDistance = sqrDistance;
            localDistance = _cumulativeDistances[i];
            isFound = true;
        }

        return isFound;
    }

    private void InitCumulativeDistances()
    {
        _cumulativeDistances.Clear();
        float accumulated = 0f;

        for (int i = 0; i < _waypoints.Count; i++)
        {
            if (0 < i)
            {
                accumulated += Vector3.Distance(_waypoints[i - 1], _waypoints[i]);
            }
            _cumulativeDistances.Add(accumulated);
        }

        Length = accumulated;
    }
}
