using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 에너지 배치에서 플레이 전에 잡을 수 있는 실수를 검사한다.
/// Room 컨텍스트 메뉴로 방 하나의 선과 코어를 한 번에 본다.
/// </summary>
public static class EnergyEditorTools
{
    [MenuItem("CONTEXT/Room/Validate Energy")]
    private static void ValidateRoomEnergy(MenuCommand command)
    {
        Room room = command.context as Room;
        if (room == null)
        {
            return;
        }

        EnergyLine[] lines = room.GetComponentsInChildren<EnergyLine>(true);
        EnergyCore[] cores = room.GetComponentsInChildren<EnergyCore>(true);
        EnergySource[] sources = room.GetComponentsInChildren<EnergySource>(true);
        EnergySwitch[] switches = room.GetComponentsInChildren<EnergySwitch>(true);
        HashSet<EnergyCore> linkedCores = new HashSet<EnergyCore>();

        int invalidCount = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            invalidCount += ReportLine(lines[i], linkedCores);
        }
        invalidCount += ReportUnlinkedCores(cores, linkedCores);
        invalidCount += ReportDuplicateTerrains(cores);
        invalidCount += EnergySupplyValidator.Report(lines, sources, switches);

        if (invalidCount == 0)
        {
            Debug.Log($"[Energy] '{room.name}' 선 {lines.Length}개, 코어 {cores.Length}개 검사 완료. 문제 없습니다.", room);
        }
    }

    [MenuItem("CONTEXT/EnergyLine/Validate Energy Line")]
    private static void ValidateEnergyLine(MenuCommand command)
    {
        EnergyLine line = command.context as EnergyLine;
        if (line == null)
        {
            return;
        }

        if (ReportLine(line, new HashSet<EnergyCore>()) == 0)
        {
            Debug.Log($"[Energy] '{line.name}' 검사 완료. 문제 없습니다.", line);
        }
    }

    private static int ReportLine(EnergyLine line, HashSet<EnergyCore> linkedCores)
    {
        EnergyLineGraph graph = line.CreateEditorGraph();
        int invalidCount = 0;

        for (int i = 0; i < graph.InvalidBranchIndices.Count; i++)
        {
            invalidCount++;
            Debug.LogWarning($"[Energy] '{line.name}'의 {graph.InvalidBranchIndices[i]}번 분기점이 메인 경로나 앞선 분기 위에 없습니다. 앞선 경로의 웨이포인트를 분기점으로 지정하세요.", line);
        }

        int coreCount = 0;
        int terrainNodeCount = 0;
        for (int p = 0; p < graph.Paths.Count; p++)
        {
            IReadOnlyList<EnergyPathNode> nodes = graph.Paths[p].Nodes;
            for (int n = 0; n < nodes.Count; n++)
            {
                if (nodes[n].NodeType == EEnergyPathNodeType.EnergyCore && nodes[n].EnergyCore != null)
                {
                    coreCount++;
                    linkedCores.Add(nodes[n].EnergyCore);
                }
                else if (nodes[n].NodeType == EEnergyPathNodeType.Terrain && nodes[n].Terrain != null)
                {
                    terrainNodeCount++;
                }
            }
        }

        if (coreCount == 0 && terrainNodeCount == 0)
        {
            invalidCount++;
            Debug.LogWarning($"[Energy] '{line.name}'이(가) 지나는 코어가 없습니다. 모든 에너지선은 코어를 하나 이상 지나야 합니다.", line);
        }

        if (0 < terrainNodeCount)
        {
            invalidCount++;
            Debug.LogWarning($"[Energy] '{line.name}'에 코어 없이 선에 바로 물린 지형 노드가 {terrainNodeCount}개 있습니다. 새 규칙에서는 지형을 코어에 등록해야 합니다.", line);
        }
        return invalidCount;
    }

    private static int ReportUnlinkedCores(EnergyCore[] cores, HashSet<EnergyCore> linkedCores)
    {
        int invalidCount = 0;
        for (int i = 0; i < cores.Length; i++)
        {
            if (linkedCores.Contains(cores[i]))
            {
                continue;
            }
            invalidCount++;
            Debug.LogWarning($"[Energy] 코어 '{cores[i].name}'을(를) 지나는 에너지선이 없습니다. 이 코어는 켜지지 않습니다.", cores[i]);
        }
        return invalidCount;
    }

    // 기획상 지형 하나는 코어 하나에만 속한다. 두 코어에 등록되면 서로 다른 선이 같은 지형을 번갈아 켜고 끈다.
    private static int ReportDuplicateTerrains(EnergyCore[] cores)
    {
        Dictionary<TerrainObject, EnergyCore> owners = new Dictionary<TerrainObject, EnergyCore>();
        int invalidCount = 0;

        for (int i = 0; i < cores.Length; i++)
        {
            IReadOnlyList<TerrainObject> terrains = cores[i].RegisteredTerrains;
            for (int j = 0; j < terrains.Count; j++)
            {
                TerrainObject terrain = terrains[j];
                if (terrain == null)
                {
                    continue;
                }

                if (owners.TryGetValue(terrain, out EnergyCore owner) && owner != cores[i])
                {
                    invalidCount++;
                    Debug.LogWarning($"[Energy] 지형 '{terrain.name}'이(가) 코어 '{owner.name}'과(와) '{cores[i].name}'에 함께 등록되어 있습니다. 지형은 코어 하나에만 등록하세요.", terrain);
                    continue;
                }
                owners[terrain] = cores[i];
            }
        }
        return invalidCount;
    }
}
