#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 에디터에서 지형의 경로 이동 데이터로 경로를 다시 구성한다.
/// 에디터에는 런타임 김믹이 없으므로 스냅 범위 기즈모와 검증 메뉴가 이 경로를 쓴다.
/// </summary>
public static class SnapPathEditorUtility
{
    public static bool TryGetPathEntry(TerrainObject terrain, out TerrainGimmickEntry entry, out PathMoveTerrainGimmickSO data)
    {
        IReadOnlyList<TerrainGimmickEntry> entries = terrain.GimmickEntries;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].GimmickData is PathMoveTerrainGimmickSO pathData &&
                entries[i].Waypoints != null && 0 < entries[i].Waypoints.Count)
            {
                entry = entries[i];
                data = pathData;
                return true;
            }
        }

        entry = null;
        data = null;
        return false;
    }

    // 플레이 중에는 지형이 이미 움직였으므로, 런타임 경로와 같게 최초 위치에서 시작한다.
    public static bool TryInitMeasure(TerrainObject terrain, TerrainPathMeasure measure)
    {
        if (terrain == null || !TryGetPathEntry(terrain, out TerrainGimmickEntry entry, out PathMoveTerrainGimmickSO data))
        {
            return false;
        }

        Vector2 initialPosition = Application.isPlaying ? terrain.InitialPosition : terrain.transform.position;
        measure.InitPath(initialPosition, data.PathEndBehaviour, entry.Waypoints);
        return true;
    }
}
#endif
