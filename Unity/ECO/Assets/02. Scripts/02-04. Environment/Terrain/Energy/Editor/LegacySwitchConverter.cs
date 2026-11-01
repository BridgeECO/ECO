using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 선에 직접 물린 스위치를, 선마다 시작점에 둔 스위치형 에너지원을 제어하는 스위치로 바꾼다.
/// 스위치의 인터랙션 타입은 그대로 둔다.
/// </summary>
public static class LegacySwitchConverter
{
    private const string SOURCE_PREFAB_PATH = "Assets/03. Prefabs/03-04. Environment/Terrain/Energy/P_EnergySource.prefab";
    private const string LINE_PREFAB_NAME = "P_EnergyLine";

    public static int Convert(Room room)
    {
        GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SOURCE_PREFAB_PATH);
        Dictionary<EnergyLine, EnergySource> lineSources = CollectLineSources(room);
        EnergySwitch[] switches = room.GetComponentsInChildren<EnergySwitch>(true);
        int convertedCount = 0;

        for (int i = 0; i < switches.Length; i++)
        {
            SerializedObject serializedSwitch = new SerializedObject(switches[i]);
            SerializedProperty legacyLines = serializedSwitch.FindProperty("_connectedLines");
            if (!HasAnyReference(legacyLines))
            {
                continue;
            }

            // 스위치가 추적할 선을 찾는 순서를 지켜야 카메라 추적이 변환 전과 같은 선을 고른다.
            SerializedProperty targetSources = serializedSwitch.FindProperty("_targetSources");
            for (int j = 0; j < legacyLines.arraySize; j++)
            {
                EnergyLine line = legacyLines.GetArrayElementAtIndex(j).objectReferenceValue as EnergyLine;
                if (line == null)
                {
                    continue;
                }

                if (!lineSources.TryGetValue(line, out EnergySource source))
                {
                    source = CreateSource(line, sourcePrefab);
                    lineSources.Add(line, source);
                }
                AddUniqueReference(targetSources, source);
            }

            legacyLines.ClearArray();
            serializedSwitch.ApplyModifiedProperties();
            convertedCount++;
        }
        return convertedCount;
    }

    // 선을 이미 공급하는 에너지원이 있으면 그것을 쓴다. 비활성 스위치처럼 같은 선을 무는 스위치가 더 있어도 에너지원이 늘지 않는다.
    private static Dictionary<EnergyLine, EnergySource> CollectLineSources(Room room)
    {
        Dictionary<EnergyLine, EnergySource> lineSources = new Dictionary<EnergyLine, EnergySource>();
        EnergySource[] sources = room.GetComponentsInChildren<EnergySource>(true);
        for (int i = 0; i < sources.Length; i++)
        {
            IReadOnlyList<EnergyLine> lines = sources[i].ConnectedLines;
            for (int j = 0; j < lines.Count; j++)
            {
                if (lines[j] != null && !lineSources.ContainsKey(lines[j]))
                {
                    lineSources.Add(lines[j], sources[i]);
                }
            }
        }
        return lineSources;
    }

    private static EnergySource CreateSource(EnergyLine line, GameObject sourcePrefab)
    {
        GameObject sourceObject = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab, line.transform.parent);
        Undo.RegisterCreatedObjectUndo(sourceObject, "Create Energy Source");
        sourceObject.name = sourcePrefab.name + GetNameSuffix(line);
        sourceObject.transform.position = GetStartPosition(line);
        sourceObject.transform.SetSiblingIndex(line.transform.GetSiblingIndex());

        EnergySource source = sourceObject.GetComponent<EnergySource>();
        SerializedObject serializedSource = new SerializedObject(source);
        serializedSource.FindProperty("_supplyPattern").intValue = (int)EEnergySupplyPattern.Switch;
        SerializedProperty lines = serializedSource.FindProperty("_connectedLines");
        lines.ClearArray();
        AddUniqueReference(lines, line);
        serializedSource.ApplyModifiedPropertiesWithoutUndo();
        return source;
    }

    private static Vector3 GetStartPosition(EnergyLine line)
    {
        IReadOnlyList<Vector3> waypoints = line.CreateEditorGraph().Paths[0].Waypoints;
        return 0 < waypoints.Count ? waypoints[0] : line.transform.position;
    }

    // 선 이름의 번호를 이어받아, 어느 선의 에너지원인지 하이어라키에서 바로 알 수 있게 한다.
    private static string GetNameSuffix(EnergyLine line)
    {
        return line.name.StartsWith(LINE_PREFAB_NAME) ? line.name.Substring(LINE_PREFAB_NAME.Length) : $"_{line.name}";
    }

    private static bool HasAnyReference(SerializedProperty references)
    {
        for (int i = 0; i < references.arraySize; i++)
        {
            if (references.GetArrayElementAtIndex(i).objectReferenceValue != null)
            {
                return true;
            }
        }
        return false;
    }

    private static void AddUniqueReference(SerializedProperty references, Object target)
    {
        for (int i = 0; i < references.arraySize; i++)
        {
            if (references.GetArrayElementAtIndex(i).objectReferenceValue == target)
            {
                return;
            }
        }

        references.InsertArrayElementAtIndex(references.arraySize);
        references.GetArrayElementAtIndex(references.arraySize - 1).objectReferenceValue = target;
    }
}
