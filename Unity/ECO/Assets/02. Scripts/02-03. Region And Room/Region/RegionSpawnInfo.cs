using UnityEngine;

/// <summary>
/// Region이 초기 배치를 결정한 결과. RegionInitialized 이벤트의 페이로드로 전달된다.
/// (Region이 RespawnManager를 직접 호출하지 않기 위한 데이터 계약)
/// </summary>
public readonly struct RegionSpawnInfo
{
    // 이어하기 진입이면 해당 세이브 포인트, 새 게임 진입이면 null.
    public readonly SavePoint SavePoint;
    public readonly Room Room;
    public readonly Vector3 Position;

    public RegionSpawnInfo(SavePoint savePoint, Room room, Vector3 position)
    {
        SavePoint = savePoint;
        Room = room;
        Position = position;
    }
}
