using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 계기판이 연결한 선과 맞는지 검사한다. 선의 코어를 바꾼 뒤 '계기판 맞추기'를 누르지 않으면
/// 코어침 수나 자리가 어긋나, 막대 끝이 코어침에 닿는 순간과 코어가 켜지는 순간이 달라진다.
/// </summary>
public static class EnergyDashboardValidator
{
    // 앵커 비율 오차. 막대 길이 10 유닛에서도 0.01 유닛 이내다.
    private const float RATIO_TOLERANCE = 0.001f;

    public static int Report(EnergyDashboard[] dashboards)
    {
        List<EnergyPathNode> coreNodes = new List<EnergyPathNode>();
        int invalidCount = 0;

        for (int i = 0; i < dashboards.Length; i++)
        {
            EnergyDashboard dashboard = dashboards[i];
            if (dashboard.Line == null)
            {
                invalidCount++;
                Debug.LogWarning($"[Energy] 계기판 '{dashboard.name}'에 연결된 에너지선이 없습니다.", dashboard);
                continue;
            }

            EnergyLineGraph graph = dashboard.Line.CreateEditorGraph();
            graph.CollectCoreNodes(coreNodes);
            if (!IsNeedleLayoutSynced(dashboard, coreNodes, graph.MaxDistance))
            {
                invalidCount++;
                Debug.LogWarning($"[Energy] 계기판 '{dashboard.name}'의 코어침이 선 '{dashboard.Line.name}'의 코어 {coreNodes.Count}개와 맞지 않습니다. 계기판의 '계기판 맞추기' 버튼을 누르세요.", dashboard);
            }
        }
        return invalidCount;
    }

    private static bool IsNeedleLayoutSynced(EnergyDashboard dashboard, List<EnergyPathNode> coreNodes, float maxDistance)
    {
        IReadOnlyList<UI_CoreNeedle> needles = dashboard.CoreNeedles;
        if (needles.Count != coreNodes.Count)
        {
            return false;
        }

        for (int i = 0; i < needles.Count; i++)
        {
            if (needles[i] == null)
            {
                return false;
            }

            float ratio = EnergyDashboardLayout.GetNeedleRatio(coreNodes[i], maxDistance);
            RectTransform needleRect = (RectTransform)needles[i].transform;
            if (RATIO_TOLERANCE < Mathf.Abs(needleRect.anchorMin.y - ratio) || RATIO_TOLERANCE < Mathf.Abs(needleRect.anchorMax.y - ratio))
            {
                return false;
            }
        }
        return true;
    }
}
