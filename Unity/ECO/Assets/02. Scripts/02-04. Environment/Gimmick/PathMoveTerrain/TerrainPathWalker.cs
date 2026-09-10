using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 경로 이동 지형의 진행 상태(목표 지점 인덱스와 진행 방향)만 담당한다.
/// Rigidbody2D를 직접 만지지 않으므로, 순간이동은 요청 플래그로만 알리고 실제 이동은 소유 김믹이 수행한다.
/// </summary>
public class TerrainPathWalker
{
    // Stop/PingPong 경로는 지형의 최초 위치를 -1번 지점으로 삼아 시작점까지 되돌아올 수 있게 한다.
    // 반면 Loop는 웨이포인트만으로 순환하며 최초 위치를 경로에 포함하지 않는다.
    private const int INITIAL_POSITION_INDEX = -1;
    private const int LOOP_START_INDEX = 1;

    private EPathEndBehaviour _endBehaviour;
    private Vector2 _initialPosition;
    private int _currentIndex;
    private bool _isMovingForward = true;
    private bool _isTeleportPending;

    public void InitPath(Vector2 initialPosition, EPathEndBehaviour endBehaviour)
    {
        _initialPosition = initialPosition;
        _endBehaviour = endBehaviour;
        ResetProgress();
    }

    public void ResetProgress()
    {
        _currentIndex = (_endBehaviour == EPathEndBehaviour.Loop) ? LOOP_START_INDEX : 0;
        _isMovingForward = true;
        _isTeleportPending = false;
    }

    public void SetForward(bool isForward)
    {
        if (_isMovingForward == isForward)
        {
            return;
        }

        // 방향이 바뀌면 방금 떠나온 지점을 다시 목표로 잡게 되므로 인덱스를 한 칸 밀어준다.
        _isMovingForward = isForward;
        _currentIndex += isForward ? 1 : -1;
    }

    public bool TryGetTargetPosition(List<Transform> waypoints, out Vector2 targetPosition)
    {
        targetPosition = _initialPosition;

        if (_currentIndex < GetMinIndex() || waypoints.Count <= _currentIndex)
        {
            return false;
        }

        if (_currentIndex == INITIAL_POSITION_INDEX)
        {
            return true;
        }

        Transform waypoint = waypoints[_currentIndex];
        if (waypoint == null)
        {
            return false;
        }

        targetPosition = waypoint.position;
        return true;
    }

    public void Advance(List<Transform> waypoints)
    {
        _currentIndex += _isMovingForward ? 1 : -1;

        if (_endBehaviour == EPathEndBehaviour.PingPong)
        {
            ReflectAtBounds(waypoints.Count);
            return;
        }

        if (_endBehaviour == EPathEndBehaviour.Loop && waypoints.Count <= _currentIndex)
        {
            _isTeleportPending = true;
            _currentIndex = LOOP_START_INDEX;
        }

        // Stop은 인덱스를 범위 밖에 그대로 두어 TryGetTargetPosition이 실패하고 순회가 끝나게 한다.
    }

    public bool TryConsumeTeleport(List<Transform> waypoints, out Vector2 teleportPosition)
    {
        teleportPosition = _initialPosition;

        if (!_isTeleportPending)
        {
            return false;
        }
        _isTeleportPending = false;

        Transform startWaypoint = waypoints[0];
        if (startWaypoint == null)
        {
            return false;
        }

        teleportPosition = startWaypoint.position;
        return true;
    }

    // 경로에 필요한 최소 웨이포인트 수는 종료 동작이 결정한다.
    // Loop은 첫 웨이포인트를 시작점으로 쓰고 두 번째 지점부터 이동하므로 2개가 필요하다.
    public int GetRequiredWaypointCount()
    {
        return (_endBehaviour == EPathEndBehaviour.Loop) ? 2 : 1;
    }

    // Loop은 최초 위치를 경로에 포함하지 않으므로 경로선도 첫 웨이포인트에서 시작해야 한다.
    public Vector2 GetPathStartPosition(List<Transform> waypoints)
    {
        if (_endBehaviour != EPathEndBehaviour.Loop || waypoints[0] == null)
        {
            return _initialPosition;
        }
        return waypoints[0].position;
    }

    private int GetMinIndex()
    {
        return (_endBehaviour == EPathEndBehaviour.Loop) ? 0 : INITIAL_POSITION_INDEX;
    }

    private void ReflectAtBounds(int waypointCount)
    {
        if (waypointCount <= _currentIndex)
        {
            _isMovingForward = false;
            _currentIndex = Mathf.Max(INITIAL_POSITION_INDEX, waypointCount - 2);
        }

        if (_currentIndex < INITIAL_POSITION_INDEX)
        {
            _isMovingForward = true;
            _currentIndex = 0;
        }
    }
}
