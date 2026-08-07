using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 저장 데이터를 현재 Region의 세이브 포인트로 해석하는 순수 로직.
/// Region에서 분리해 방 전환 책임과 세이브 해석 책임을 나눈다.
/// </summary>
public class RegionSaveResolver
{
    private readonly IReadOnlyList<Room> _rooms;
    private readonly ERegions _regionType;

    public RegionSaveResolver(IReadOnlyList<Room> rooms, ERegions regionType)
    {
        _rooms = rooms;
        _regionType = regionType;
    }

    // 저장 데이터가 현재 씬의 Region과 일치하는지 검사한다.
    public bool IsValidContinueSaveData(SaveData saveData)
    {
        return saveData is not null && saveData.Region == _regionType;
    }

    /// <summary>
    /// 저장된 좌표에 해당하는 세이브 포인트를 찾는다.
    /// 허용 반경 안에서 가장 가까운 것을 반환하며, 없으면 null.
    /// </summary>
    public SavePoint FindSavePointAt(Vector3 position)
    {
        SavePoint nearestSavePoint = null;
        float nearestSqrDistance = Region.SAVE_POINT_MATCH_DISTANCE * Region.SAVE_POINT_MATCH_DISTANCE;

        for (int i = 0; i < _rooms.Count; i++)
        {
            if (_rooms[i] == null)
            {
                continue;
            }

            // 비활성 Room은 Awake가 실행되지 않아 목록이 null일 수 있다.
            IReadOnlyList<SavePoint> savePoints = _rooms[i].SavePoints;
            if (savePoints == null)
            {
                continue;
            }

            for (int j = 0; j < savePoints.Count; j++)
            {
                float sqrDistance = ((Vector2)(savePoints[j].RespawnPosition - position)).sqrMagnitude;
                if (nearestSqrDistance <= sqrDistance)
                {
                    continue;
                }
                nearestSqrDistance = sqrDistance;
                nearestSavePoint = savePoints[j];
            }
        }
        return nearestSavePoint;
    }
}
