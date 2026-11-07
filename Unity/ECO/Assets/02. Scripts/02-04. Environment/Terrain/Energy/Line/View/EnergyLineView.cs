using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 에너지선을 화면에 그린다. 분기마다 늘 보이는 바탕선을 깔고, 그 위에 에너지가 지나간 구간과 에너지구를 올린다.
/// 판정은 EnergyLine이 맡고, 이 컴포넌트는 그 결과인 펄스 목록과 경로를 읽어 그리기만 한다.
/// </summary>
public class EnergyLineView : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private EnergyLine _line;

    [Tooltip("분기마다 하나씩 두는 바탕선입니다. 메인 경로, 유효한 분기 순서로 둡니다. 분기를 바꾼 뒤에는 '바탕선 맞추기' 버튼을 누르세요.")]
    [SerializeField]
    private List<LineRenderer> _baseLineRenderers = new List<LineRenderer>();

    [Foldout("Project")]
    [Tooltip("'바탕선 맞추기'가 분기 바탕선을 새로 만들 때 쓰는 프리팹입니다. 바탕선의 머티리얼과 굵기는 이 프리팹에서 바꿉니다.")]
    [SerializeField]
    private LineRenderer _baseLinePrefab;

    private EnergyLineRendererDrawer _baseLineDrawer = new EnergyLineRendererDrawer();
    private EnergyLineOverlayDrawer _overlayDrawer = new EnergyLineOverlayDrawer();
    private EnergyOrbDrawer _orbDrawer = new EnergyOrbDrawer();

    public IReadOnlyList<LineRenderer> BaseLineRenderers => _baseLineRenderers;

    private void Start()
    {
        InitBaseLines();
    }

    // 펄스는 EnergyLine.Update에서 움직이므로, 같은 프레임의 결과를 그리도록 LateUpdate에서 갱신한다.
    private void LateUpdate()
    {
        IReadOnlyList<EnergySegment> segments = _line.Segments;
        IReadOnlyList<EnergyLineBranchPath> paths = _line.Graph.Paths;
        _overlayDrawer.Refresh(segments, paths);
        _orbDrawer.Refresh(segments, paths);
    }

    private void OnDisable()
    {
        ReleaseVisuals();
    }

    // 선은 움직이지 않으므로 바탕선은 시작할 때 한 번만 그린다. 남는 바탕선은 비워 둔다.
    private void InitBaseLines()
    {
        IReadOnlyList<EnergyLineBranchPath> paths = _line.Graph.Paths;
        for (int i = 0; i < _baseLineRenderers.Count; i++)
        {
            LineRenderer baseLine = _baseLineRenderers[i];
            if (baseLine == null)
            {
                continue;
            }

            if (paths.Count <= i)
            {
                baseLine.positionCount = 0;
                continue;
            }
            _baseLineDrawer.UpdateSegmentRenderer(baseLine, paths[i], 0f, paths[i].Length);
        }

        if (_baseLineRenderers.Count < paths.Count)
        {
            Debug.LogWarning($"[EnergyLineView] {name}의 바탕선이 경로보다 적어 일부 분기가 보이지 않습니다. '바탕선 맞추기' 버튼을 누르세요.", this);
        }
    }

    private void ReleaseVisuals()
    {
        _overlayDrawer.ReleaseAll();
        _orbDrawer.ReleaseAll();
    }

#if UNITY_EDITOR
    // 분기를 추가하거나 지운 뒤 누르면 바탕선 자식을 경로 수에 맞춰 만들거나 지운다.
    [Button("바탕선 맞추기")]
    private void SyncBaseLines()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("바탕선은 Play Mode가 아닐 때 맞출 수 있습니다.", this);
            return;
        }

        int pathCount = _line.CreateEditorGraph().Paths.Count;
        UnityEditor.Undo.RecordObject(this, "Sync Base Lines");
        _baseLineRenderers.RemoveAll(baseLine => baseLine == null);
        while (_baseLineRenderers.Count < pathCount)
        {
            AddBaseLine();
        }
        while (pathCount < _baseLineRenderers.Count)
        {
            RemoveLastBaseLine();
        }
        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }

    private void AddBaseLine()
    {
        GameObject baseLine = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(_baseLinePrefab.gameObject, transform);
        baseLine.name = _baseLineRenderers.Count == 0 ? "BaseLine_Main" : $"BaseLine_Branch{_baseLineRenderers.Count}";
        UnityEditor.Undo.RegisterCreatedObjectUndo(baseLine, "Sync Base Lines");
        _baseLineRenderers.Add(baseLine.GetComponent<LineRenderer>());
    }

    private void RemoveLastBaseLine()
    {
        int lastIndex = _baseLineRenderers.Count - 1;
        LineRenderer baseLine = _baseLineRenderers[lastIndex];
        _baseLineRenderers.RemoveAt(lastIndex);
        UnityEditor.Undo.DestroyObjectImmediate(baseLine.gameObject);
    }
#endif
}
