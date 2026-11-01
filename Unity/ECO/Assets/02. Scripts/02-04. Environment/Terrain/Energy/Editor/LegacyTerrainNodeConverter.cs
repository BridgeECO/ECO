using UnityEditor;
using UnityEngine;

/// <summary>
/// 코어 없이 선에 바로 물린 지형 노드를, 같은 판정 지점을 가진 코어 노드로 바꾼다.
/// 지형 노드는 지형의 두 판정 지점을 선의 키포인트로 썼다. 코어의 두 판정 지점을 그 자리에 두면 경로와 판정 거리가 그대로다.
/// </summary>
public static class LegacyTerrainNodeConverter
{
    private const string CORE_PREFAB_PATH = "Assets/03. Prefabs/03-04. Environment/Terrain/Energy/P_EnergyCore.prefab";

    public static int Convert(Room room)
    {
        GameObject corePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CORE_PREFAB_PATH);
        EnergyLine[] lines = room.GetComponentsInChildren<EnergyLine>(true);
        int convertedCount = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            SerializedObject serializedLine = new SerializedObject(lines[i]);
            convertedCount += ConvertNodes(serializedLine.FindProperty("_pathNodes"), lines[i], corePrefab);

            SerializedProperty branches = serializedLine.FindProperty("_branches");
            for (int j = 0; j < branches.arraySize; j++)
            {
                SerializedProperty branchNodes = branches.GetArrayElementAtIndex(j).FindPropertyRelative("_pathNodes");
                convertedCount += ConvertNodes(branchNodes, lines[i], corePrefab);
            }
            serializedLine.ApplyModifiedProperties();
        }
        return convertedCount;
    }

    private static int ConvertNodes(SerializedProperty nodes, EnergyLine line, GameObject corePrefab)
    {
        int convertedCount = 0;
        for (int i = 0; i < nodes.arraySize; i++)
        {
            SerializedProperty node = nodes.GetArrayElementAtIndex(i);
            SerializedProperty nodeType = node.FindPropertyRelative("_nodeType");
            SerializedProperty terrainProperty = node.FindPropertyRelative("_terrain");
            TerrainObject terrain = terrainProperty.objectReferenceValue as TerrainObject;
            if (nodeType.intValue != (int)EEnergyPathNodeType.Terrain || terrain == null)
            {
                continue;
            }

            nodeType.intValue = (int)EEnergyPathNodeType.EnergyCore;
            node.FindPropertyRelative("_energyCore").objectReferenceValue = CreateCore(terrain, line, corePrefab);
            terrainProperty.objectReferenceValue = null;
            convertedCount++;
        }
        return convertedCount;
    }

    private static EnergyCore CreateCore(TerrainObject terrain, EnergyLine line, GameObject corePrefab)
    {
        GameObject coreObject = (GameObject)PrefabUtility.InstantiatePrefab(corePrefab, line.transform.parent);
        Undo.RegisterCreatedObjectUndo(coreObject, "Create Energy Core");
        coreObject.name = $"{corePrefab.name}_{terrain.name}";
        coreObject.transform.SetSiblingIndex(line.transform.GetSiblingIndex() + 1);

        EnergyCore core = coreObject.GetComponent<EnergyCore>();
        Vector3 activationPoint = GetPointOrCenter(terrain.ActivationPosition, terrain);
        coreObject.transform.position = activationPoint;
        core.ActivationPosition.position = activationPoint;
        core.DeactivationPosition.position = GetPointOrCenter(terrain.DeactivationPosition, terrain);

        SerializedObject serializedCore = new SerializedObject(core);
        SerializedProperty terrains = serializedCore.FindProperty("_registeredTerrains");
        terrains.ClearArray();
        terrains.InsertArrayElementAtIndex(0);
        terrains.GetArrayElementAtIndex(0).objectReferenceValue = terrain;
        serializedCore.ApplyModifiedPropertiesWithoutUndo();
        return core;
    }

    private static Vector3 GetPointOrCenter(Transform point, TerrainObject terrain)
    {
        return point != null ? point.position : terrain.transform.position;
    }
}
