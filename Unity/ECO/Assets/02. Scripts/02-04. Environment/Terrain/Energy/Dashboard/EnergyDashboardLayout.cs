#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 계기판 배치를 선에 맞춘다. 막대 길이로 계기판 높이를 정하고, 선이 지나는 코어마다 코어침을 만들어
/// 코어가 켜지는 거리 ÷ 가장 긴 경로 거리 높이에 둔다. 코어침 수는 레벨 데이터로 정해지므로 풀 대신 자식으로 미리 만든다.
/// </summary>
public static class EnergyDashboardLayout
{
    private const string UNDO_NAME = "Sync Energy Dashboard";

    public static void Sync(EnergyDashboard dashboard)
    {
        // 프리팹 원본에는 연결한 선이 없으므로 배치된 계기판에서만 맞춘다.
        if (dashboard == null || Application.isPlaying || dashboard.Line == null || dashboard.NeedleArea == null)
        {
            return;
        }

        EnergyLineGraph graph = dashboard.Line.CreateEditorGraph();
        List<EnergyPathNode> coreNodes = new List<EnergyPathNode>();
        graph.CollectCoreNodes(coreNodes);

        SyncCanvasHeight(dashboard);
        SyncNeedleCount(dashboard, coreNodes.Count);
        SyncNeedlePositions(dashboard, coreNodes, graph.MaxDistance);
    }

    // 막대의 펄스 머리도 같은 비율로 그리므로, 막대 끝이 코어침에 닿는 프레임에 코어가 켜진다.
    public static float GetNeedleRatio(EnergyPathNode coreNode, float maxDistance)
    {
        return (0f < maxDistance) ? Mathf.Clamp01(coreNode.ActivationCenterDistance / maxDistance) : 0f;
    }

    // 막대 위아래 여백은 프리팹이 정한다. 캔버스 높이에서 지금 막대 높이를 빼 여백을 구하고 새 막대 길이를 더한다.
    private static void SyncCanvasHeight(EnergyDashboard dashboard)
    {
        RectTransform needleArea = dashboard.NeedleArea;
        RectTransform canvasRect = (RectTransform)needleArea.GetComponentInParent<Canvas>().rootCanvas.transform;
        float barHeight = dashboard.BarLength / canvasRect.lossyScale.y;
        float height = canvasRect.sizeDelta.y - needleArea.rect.height + barHeight;
        if (Mathf.Abs(canvasRect.sizeDelta.y - height) < 0.01f)
        {
            return;
        }

        Undo.RecordObject(canvasRect, UNDO_NAME);
        canvasRect.sizeDelta = new Vector2(canvasRect.sizeDelta.x, height);
        PrefabUtility.RecordPrefabInstancePropertyModifications(canvasRect);
    }

    private static void SyncNeedleCount(EnergyDashboard dashboard, int coreCount)
    {
        SerializedObject serialized = new SerializedObject(dashboard);
        SerializedProperty needles = serialized.FindProperty("_coreNeedles");
        RemoveMissingNeedles(needles);

        while (needles.arraySize < coreCount)
        {
            GameObject needle = (GameObject)PrefabUtility.InstantiatePrefab(dashboard.CoreNeedlePrefab.gameObject, dashboard.NeedleArea);
            Undo.RegisterCreatedObjectUndo(needle, UNDO_NAME);
            needles.arraySize++;
            needles.GetArrayElementAtIndex(needles.arraySize - 1).objectReferenceValue = needle.GetComponent<UI_CoreNeedle>();
        }

        while (coreCount < needles.arraySize)
        {
            int lastIndex = needles.arraySize - 1;
            UI_CoreNeedle needle = needles.GetArrayElementAtIndex(lastIndex).objectReferenceValue as UI_CoreNeedle;
            needles.DeleteArrayElementAtIndex(lastIndex);
            Undo.DestroyObjectImmediate(needle.gameObject);
        }

        serialized.ApplyModifiedProperties();
    }

    private static void RemoveMissingNeedles(SerializedProperty needles)
    {
        for (int i = needles.arraySize - 1; 0 <= i; i--)
        {
            if (needles.GetArrayElementAtIndex(i).objectReferenceValue == null)
            {
                needles.DeleteArrayElementAtIndex(i);
            }
        }
    }

    private static void SyncNeedlePositions(EnergyDashboard dashboard, List<EnergyPathNode> coreNodes, float maxDistance)
    {
        IReadOnlyList<UI_CoreNeedle> needles = dashboard.CoreNeedles;
        for (int i = 0; i < needles.Count && i < coreNodes.Count; i++)
        {
            RectTransform needleRect = (RectTransform)needles[i].transform;
            Vector2 anchor = new Vector2(0.5f, GetNeedleRatio(coreNodes[i], maxDistance));
            string needleName = $"CoreNeedle_{coreNodes[i].EnergyCore.name}";
            if (needleRect.anchorMin == anchor && needleRect.anchorMax == anchor &&
                needleRect.anchoredPosition == Vector2.zero && needles[i].name == needleName)
            {
                continue;
            }

            Undo.RecordObjects(new Object[] { needleRect, needles[i].gameObject }, UNDO_NAME);
            needleRect.anchorMin = anchor;
            needleRect.anchorMax = anchor;
            needleRect.anchoredPosition = Vector2.zero;
            needles[i].gameObject.name = needleName;
            PrefabUtility.RecordPrefabInstancePropertyModifications(needleRect);
            PrefabUtility.RecordPrefabInstancePropertyModifications(needles[i].gameObject);
        }
    }
}
#endif
