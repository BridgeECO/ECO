using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스냅포인트 배치를 검사한다(기획서 v15). 스냅포인트는 이동형 코어 하나에 딸리고,
/// 그 코어에 묶인 지형의 웨이포인트 경로 위에 있어야 한다. 묶인 지형은 같은 거리만큼 함께 안착한다.
/// </summary>
public static class SnapPointValidator
{
    public static int Report(EnergyCore[] cores)
    {
        Dictionary<SnapPoint, EnergyCore> owners = new Dictionary<SnapPoint, EnergyCore>();
        int invalidCount = 0;

        for (int i = 0; i < cores.Length; i++)
        {
            EnergyCore core = cores[i];
            if (core.SnapPoints.Count == 0)
            {
                continue;
            }

            invalidCount += ReportOwners(core, owners);
            if (!core.IsMovableCore)
            {
                invalidCount++;
                Debug.LogWarning($"[Energy] 상태형 코어 '{core.name}'에 스냅포인트 {core.SnapPoints.Count}개가 등록되어 있습니다. 스냅포인트는 위치를 옮기는 지형의 코어에서만 동작합니다.", core);
                continue;
            }

            List<TerrainPathMeasure> measures = new List<TerrainPathMeasure>();
            invalidCount += ReportTerrains(core, measures);
            invalidCount += ReportSnapPoints(core, measures);
        }
        return invalidCount;
    }

    private static int ReportOwners(EnergyCore core, Dictionary<SnapPoint, EnergyCore> owners)
    {
        int invalidCount = 0;
        for (int i = 0; i < core.SnapPoints.Count; i++)
        {
            SnapPoint snapPoint = core.SnapPoints[i];
            if (snapPoint == null)
            {
                continue;
            }

            if (owners.TryGetValue(snapPoint, out EnergyCore owner) && owner != core)
            {
                invalidCount++;
                Debug.LogWarning($"[Energy] 스냅포인트 '{snapPoint.name}'이(가) 코어 '{owner.name}'과(와) '{core.name}'에 함께 등록되어 있습니다. 스냅포인트는 코어 하나에만 등록하세요.", snapPoint);
                continue;
            }
            owners[snapPoint] = core;
        }
        return invalidCount;
    }

    // 안착시킬 수 있는 지형의 경로를 모으면서, 안착에서 빠지는 지형과 경로 구성이 다른 지형을 알린다.
    private static int ReportTerrains(EnergyCore core, List<TerrainPathMeasure> measures)
    {
        int invalidCount = 0;
        TerrainObject reference = null;
        TerrainGimmickEntry referenceEntry = null;
        PathMoveTerrainGimmickSO referenceData = null;

        for (int i = 0; i < core.RegisteredTerrains.Count; i++)
        {
            TerrainObject terrain = core.RegisteredTerrains[i];
            if (terrain == null || !SnapPathEditorUtility.TryGetPathEntry(terrain, out TerrainGimmickEntry entry, out PathMoveTerrainGimmickSO data))
            {
                continue;
            }

            if (data.IsReturningOnDeactivate)
            {
                invalidCount++;
                Debug.LogWarning($"[Energy] 코어 '{core.name}'의 지형 '{terrain.name}'은(는) 비활성 시 역주행 복귀 지형이라 멈추지 않으므로 스냅포인트에 안착하지 않습니다.", terrain);
                continue;
            }

            if (reference == null)
            {
                reference = terrain;
                referenceEntry = entry;
                referenceData = data;
            }
            else if (entry.Waypoints.Count != referenceEntry.Waypoints.Count || data.PathEndBehaviour != referenceData.PathEndBehaviour)
            {
                invalidCount++;
                Debug.LogWarning($"[Energy] 코어 '{core.name}'에 묶인 지형 '{terrain.name}'(웨이포인트 {entry.Waypoints.Count}개, {data.PathEndBehaviour})의 경로 구성이 '{reference.name}'(웨이포인트 {referenceEntry.Waypoints.Count}개, {referenceData.PathEndBehaviour})과(와) 다릅니다. 묶인 지형은 같은 거리만큼 함께 안착하므로 경로 구성을 맞추세요.", terrain);
            }

            TerrainPathMeasure measure = new TerrainPathMeasure();
            SnapPathEditorUtility.TryInitMeasure(terrain, measure);
            measures.Add(measure);
        }
        return invalidCount;
    }

    private static int ReportSnapPoints(EnergyCore core, List<TerrainPathMeasure> measures)
    {
        int invalidCount = 0;
        for (int i = 0; i < core.SnapPoints.Count; i++)
        {
            SnapPoint snapPoint = core.SnapPoints[i];
            if (snapPoint == null)
            {
                invalidCount++;
                Debug.LogWarning($"[Energy] 코어 '{core.name}'의 스냅포인트 목록 {i}번이 비어 있습니다.", core);
                continue;
            }

            if (snapPoint.SnapDistance <= 0f)
            {
                invalidCount++;
                Debug.LogWarning($"[Energy] 스냅포인트 '{snapPoint.name}'의 스냅 거리가 0입니다. 지형이 정확히 그 자리에 멈출 때만 안착합니다.", snapPoint);
            }

            if (!IsOnAnyPath(snapPoint, measures))
            {
                invalidCount++;
                Debug.LogWarning($"[Energy] 스냅포인트 '{snapPoint.name}'이(가) 코어 '{core.name}' 지형의 웨이포인트 경로 위({SnapPoint.PATH_TOLERANCE} 이내)에 없어 동작하지 않습니다. 지형이 지나는 경로 위로 옮기세요.", snapPoint);
            }
        }
        return invalidCount;
    }

    private static bool IsOnAnyPath(SnapPoint snapPoint, List<TerrainPathMeasure> measures)
    {
        for (int i = 0; i < measures.Count; i++)
        {
            if (measures[i].TryFindDistance(snapPoint.transform.position, SnapPoint.PATH_TOLERANCE, out float _))
            {
                return true;
            }
        }
        return false;
    }
}
