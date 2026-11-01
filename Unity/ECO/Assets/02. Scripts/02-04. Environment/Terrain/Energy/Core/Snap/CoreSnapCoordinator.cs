using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이동형 코어의 스냅포인트 안착을 맡는다(기획서 v15 스냅포인트 규칙).
/// 코어 신호로 지형이 멈추면 (지형 × 스냅포인트) 가운데 범위 안에서 가장 가까운 조합을 고르고,
/// 그 거리만큼 코어에 묶인 지형을 모두 같은 속도로 동시에 옮긴다.
/// 거리는 지형 웨이포인트 경로를 따라 잰 값이다. 옮기는 거리는 지형마다 진행 방향 기준(+는 가던 쪽, -는 온 쪽)으로 나눠 주어,
/// 서로 반대로 오가는 리프트도 시간을 조금 당기거나 되돌린 것처럼 함께 움직인다.
/// </summary>
public class CoreSnapCoordinator
{
    private IReadOnlyList<TerrainObject> _terrains;
    private IReadOnlyList<SnapPoint> _snapPoints;

    // 코어 상태가 바뀌기 직전에 움직이던 지형. 이번 신호로 멈춘 지형만 골라 안착시키기 위해 기억한다.
    // 리스폰 리셋으로 이미 멈춰 최초 위치로 돌아간 지형은 여기에 들지 않으므로, 리셋 직후에 엉뚱하게 안착하지 않는다.
    private List<TerrainObject> _movingTerrains = new List<TerrainObject>();
    private PathMoveTerrainGimmick _dockingGimmick;

    public bool IsDocked => !ReferenceEquals(_dockingGimmick, null) && _dockingGimmick.IsDocked;
    public bool IsSettling => !ReferenceEquals(_dockingGimmick, null) && _dockingGimmick.IsSettling;

    public CoreSnapCoordinator(IReadOnlyList<TerrainObject> terrains, IReadOnlyList<SnapPoint> snapPoints)
    {
        _terrains = terrains;
        _snapPoints = snapPoints;
    }

    public void CaptureMovingTerrains()
    {
        _movingTerrains.Clear();
        for (int i = 0; i < _terrains.Count; i++)
        {
            if (TryGetSettleableGimmick(_terrains[i], out PathMoveTerrainGimmick gimmick) && gimmick.IsActivated)
            {
                _movingTerrains.Add(_terrains[i]);
            }
        }
    }

    public void SettleStoppedTerrains()
    {
        RemoveStillMovingTerrains();
        if (_movingTerrains.Count == 0)
        {
            return;
        }

        _dockingGimmick = null;
        if (TryFindClosestSnap(_movingTerrains, out PathMoveTerrainGimmick dockingGimmick, out SnapPoint snapPoint, out float travelOffset))
        {
            SettleTerrains(travelOffset, dockingGimmick.MoveSpeed * snapPoint.SettleSpeedRatio);
            _dockingGimmick = dockingGimmick;
        }
        _movingTerrains.Clear();
    }

    // 지금 멈춘다면 안착할 스냅포인트에 얼마나 가까운지. 범위 경계에서 0, 스냅포인트 위에서 1이다.
    public float GetProximity()
    {
        if (!TryFindClosestSnap(_terrains, out _, out SnapPoint snapPoint, out float travelOffset))
        {
            return 0f;
        }
        return (0f < snapPoint.SnapDistance) ? 1f - Mathf.Abs(travelOffset) / snapPoint.SnapDistance : 1f;
    }

    private void RemoveStillMovingTerrains()
    {
        for (int i = _movingTerrains.Count - 1; 0 <= i; i--)
        {
            _movingTerrains[i].TryGetPathMoveGimmick(out PathMoveTerrainGimmick gimmick);
            if (gimmick.IsActivated)
            {
                _movingTerrains.RemoveAt(i);
            }
        }
    }

    // 묶인 지형은 각자의 경로 길이를 넘지 않는 범위에서, 고른 조합과 같은 거리와 같은 속도로 함께 움직인다.
    private void SettleTerrains(float travelOffset, float speed)
    {
        if (speed <= 0f)
        {
            return;
        }

        for (int i = 0; i < _movingTerrains.Count; i++)
        {
            TerrainObject terrain = _movingTerrains[i];
            terrain.TryGetPathMoveGimmick(out PathMoveTerrainGimmick gimmick);
            float pathOffset = gimmick.IsMovingForward ? travelOffset : -travelOffset;
            float targetDistance = Mathf.Clamp(gimmick.GetCurrentDistance(terrain) + pathOffset, 0f, gimmick.Measure.TotalLength);
            gimmick.Settle(terrain, targetDistance, speed);
        }
    }

    private bool TryFindClosestSnap(IReadOnlyList<TerrainObject> terrains, out PathMoveTerrainGimmick closestGimmick,
        out SnapPoint closestPoint, out float closestTravelOffset)
    {
        closestGimmick = null;
        closestPoint = null;
        closestTravelOffset = 0f;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < terrains.Count; i++)
        {
            if (!TryGetSettleableGimmick(terrains[i], out PathMoveTerrainGimmick gimmick))
            {
                continue;
            }

            float currentDistance = gimmick.GetCurrentDistance(terrains[i]);
            for (int j = 0; j < _snapPoints.Count; j++)
            {
                SnapPoint snapPoint = _snapPoints[j];
                if (snapPoint == null ||
                    !gimmick.Measure.TryFindDistance(snapPoint.transform.position, SnapPoint.PATH_TOLERANCE, out float snapDistance))
                {
                    continue;
                }

                float pathOffset = snapDistance - currentDistance;
                float absOffset = Mathf.Abs(pathOffset);
                if (snapPoint.SnapDistance < absOffset || closestDistance <= absOffset)
                {
                    continue;
                }

                closestDistance = absOffset;
                closestGimmick = gimmick;
                closestPoint = snapPoint;
                closestTravelOffset = gimmick.IsMovingForward ? pathOffset : -pathOffset;
            }
        }
        return !ReferenceEquals(closestGimmick, null);
    }

    private bool TryGetSettleableGimmick(TerrainObject terrain, out PathMoveTerrainGimmick gimmick)
    {
        gimmick = null;
        return terrain != null && terrain.TryGetPathMoveGimmick(out gimmick) && gimmick.CanSettle;
    }
}
