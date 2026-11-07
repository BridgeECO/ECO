using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 경로 이동 지형의 경로를 꺾은선으로 보고, 시작점에서부터 경로를 따라 잰 거리로 위치를 다룬다.
/// 꼭짓점 번호는 TerrainPathWalker와 같다. Stop/PingPong은 최초 위치가 0번이고, Loop은 첫 웨이포인트가 0번이다.
/// 웨이포인트는 배치에서 움직이지 않지만 에디터에서는 계속 옮겨지므로, 누적 거리를 저장하지 않고 필요할 때 계산한다.
/// </summary>
public class TerrainPathMeasure
{
    // 이동기의 도착 판정(0.001)보다 작아야, 꼭짓점 위에 맞춘 지형이 다시 출발할 때 도착 처리부터 한다.
    private const float VERTEX_EPSILON = 0.0005f;

    private Vector2 _initialPosition;
    private List<Transform> _waypoints;
    private int _firstWaypointVertex;

    public int VertexCount => (_waypoints == null) ? 0 : _waypoints.Count + _firstWaypointVertex;
    public float TotalLength => GetVertexDistance(VertexCount - 1);

    public void InitPath(Vector2 initialPosition, EPathEndBehaviour endBehaviour, List<Transform> waypoints)
    {
        _initialPosition = initialPosition;
        _waypoints = waypoints;
        _firstWaypointVertex = (endBehaviour == EPathEndBehaviour.Loop) ? 0 : 1;
    }

    // 비어 있는 웨이포인트는 앞 꼭짓점과 같은 자리로 본다. 지형은 그 지점에서 멈추므로 거리를 더하지 않는다.
    public Vector2 GetVertex(int vertex)
    {
        for (int i = vertex - _firstWaypointVertex; 0 <= i; i--)
        {
            if (_waypoints[i] != null)
            {
                return _waypoints[i].position;
            }
        }
        return _initialPosition;
    }

    public float GetVertexDistance(int vertex)
    {
        float distance = 0f;
        for (int i = 1; i <= vertex; i++)
        {
            distance += Vector2.Distance(GetVertex(i - 1), GetVertex(i));
        }
        return distance;
    }

    // 지형이 지금 지나는 구간을 알 때 쓴다. 경로가 꺾이거나 겹쳐도 그 구간 위의 거리만 계산한다.
    public float GetDistanceOnSegment(int fromVertex, Vector2 position)
    {
        ProjectOnSegment(GetVertex(fromVertex), GetVertex(fromVertex + 1), position, out float along);
        return GetVertexDistance(fromVertex) + along;
    }

    // 스냅포인트처럼 경로 위에 놓은 점의 거리. 경로에서 tolerance보다 멀면 경로 위에 없는 것으로 본다.
    // 경로가 같은 자리를 두 번 지나면 먼저 지나는 쪽을 쓴다.
    public bool TryFindDistance(Vector2 point, float tolerance, out float distance)
    {
        distance = 0f;
        float closestSqrOffset = tolerance * tolerance;
        bool isFound = false;
        float accumulated = 0f;
        Vector2 from = GetVertex(0);

        for (int i = 1; i < VertexCount; i++)
        {
            Vector2 to = GetVertex(i);
            Vector2 projected = ProjectOnSegment(from, to, point, out float along);
            float sqrOffset = (point - projected).sqrMagnitude;
            if (sqrOffset < closestSqrOffset)
            {
                closestSqrOffset = sqrOffset;
                distance = accumulated + along;
                isFound = true;
            }

            accumulated += Vector2.Distance(from, to);
            from = to;
        }
        return isFound;
    }

    public Vector2 GetPosition(float distance)
    {
        float accumulated = 0f;
        Vector2 from = GetVertex(0);

        for (int i = 1; i < VertexCount; i++)
        {
            Vector2 to = GetVertex(i);
            float length = Vector2.Distance(from, to);
            if (distance <= accumulated + length)
            {
                float t = (0f < length) ? (distance - accumulated) / length : 0f;
                return Vector2.Lerp(from, to, t);
            }

            accumulated += length;
            from = to;
        }
        return from;
    }

    // 진행 방향으로 아직 지나지 않은 첫 꼭짓점. 꼭짓점 위에 있으면 그 꼭짓점을 돌려준다.
    // 그러면 다시 출발할 때 기존 순회가 도착 처리부터 해서 PingPong 반사와 Loop 순간이동을 그대로 따른다.
    public int FindNextVertex(float distance, bool isMovingForward)
    {
        int lastVertex = VertexCount - 1;
        int passedVertex = 0;
        float accumulated = 0f;

        for (int i = 0; i <= lastVertex; i++)
        {
            if (0 < i)
            {
                accumulated += Vector2.Distance(GetVertex(i - 1), GetVertex(i));
            }

            if (isMovingForward && distance - VERTEX_EPSILON <= accumulated)
            {
                return i;
            }

            if (distance + VERTEX_EPSILON < accumulated)
            {
                break;
            }
            passedVertex = i;
        }
        return isMovingForward ? lastVertex : passedVertex;
    }

    private Vector2 ProjectOnSegment(Vector2 from, Vector2 to, Vector2 point, out float along)
    {
        Vector2 segment = to - from;
        float sqrLength = segment.sqrMagnitude;
        float t = (0f < sqrLength) ? Mathf.Clamp01(Vector2.Dot(point - from, segment) / sqrLength) : 0f;
        along = t * Mathf.Sqrt(sqrLength);
        return from + segment * t;
    }
}
