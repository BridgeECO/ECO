using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class EnergyLine : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Transform _startPoint;

    [SerializeField]
    private Transform _endPoint;

    [SerializeField]
    private List<EnergyPathNode> _pathNodes = new List<EnergyPathNode>();

    [Tooltip("메인 경로나 앞선 분기 위의 점에서 갈라져 나가는 추가 경로입니다. 모든 분기에서 에너지가 같은 속도로 동시에 나아갑니다.")]
    [SerializeField]
    private List<EnergyLineBranch> _branches = new List<EnergyLineBranch>();

    [Foldout("Settings")]
    [Tooltip("에너지 라인 곡선의 분할 해상도 (적정값 : 0 ~ 20)")]
    [SerializeField]
    private int _curveResolution = 20;

    [Tooltip("스위치 최초 발동 시 에너지 구를 카메라가 추적하는 기능 활성화 여부")]
    [SerializeField]
    private bool _useTracking = false;

    [Tooltip("에너지 조각의 이동 속도")]
    [SerializeField]
    private float _energySpeed = 10f;

    [Tooltip("스플라인 곡선 경로의 장력 및 휘어짐 강도 (적정값: 0.0 ~ 1.2)")]
    [SerializeField]
    private float _splineTension = 1f;

    private EnergyLineGraph _graph = new EnergyLineGraph();
    private EnergySegmentController _segmentController = new EnergySegmentController();
    private EnergyNodeActivator _nodeActivator = new EnergyNodeActivator();

    public bool UseTracking => _useTracking;
    public bool IsSupplied { get; private set; }
    public float TotalDistance => _graph.MaxDistance;
    public EnergyLineGraph Graph => _graph;
    public IReadOnlyList<EnergySegment> Segments => _segmentController.ActiveSegments;

    private void Awake()
    {
        InitPathNodes();
        InitGraph();
    }

    private void Update()
    {
        _segmentController.UpdateSegments(Time.deltaTime, _graph.MaxDistance, _energySpeed);
        _nodeActivator.Refresh(_segmentController.ActiveSegments);
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        EnergyLineGizmoDrawer.Draw(Application.isPlaying ? _graph : CreateEditorGraph());
#endif
    }

    public void SetSwitchState(bool isOn)
    {
        IsSupplied = isOn;
        if (isOn)
        {
            _segmentController.StartNewSegment();
            return;
        }
        _segmentController.StopCurrentSegmentImmediately();
    }

    // 처음부터 공급 중인 에너지원은 흘려보내는 연출 없이 선을 가득 채운 채로 시작한다.
    public void FillImmediately()
    {
        IsSupplied = true;
        _segmentController.FillImmediately(_graph.MaxDistance);
    }

    // 리스폰 리셋처럼 흘려보내는 연출 없이 즉시 비워야 할 때 쓴다. 공급 상태는 호출한 쪽이 다시 정한다.
    // 펄스가 없어지면 EnergyLineView가 다음 LateUpdate에서 오버레이와 에너지구를 반납한다.
    public void ResetEnergy()
    {
        IsSupplied = false;
        _segmentController.ClearImmediately();
        _nodeActivator.DeactivateAll();
    }

    // HeadDistance를 가장 긴 경로 위에서 보간하여 현재 에너지 구의 월드 포지션을 반환한다.
    public bool TryGetSegmentHeadWorldPosition(out Vector3 worldPos)
    {
        IReadOnlyList<EnergySegment> segments = _segmentController.ActiveSegments;
        if (segments.Count == 0)
        {
            worldPos = Vector3.zero;
            return false;
        }

        return _graph.TrySampleOnLongestPath(segments[segments.Count - 1].HeadDistance, out worldPos);
    }

    // 에너지 구가 라인 끝에 도달했는지 판별하기 위해 HeadDistance를 직접 반환한다.
    public bool TryGetSegmentHeadDistance(out float headDistance)
    {
        IReadOnlyList<EnergySegment> segments = _segmentController.ActiveSegments;
        if (segments.Count == 0)
        {
            headDistance = 0f;
            return false;
        }

        headDistance = segments[segments.Count - 1].HeadDistance;
        return true;
    }

#if UNITY_EDITOR
    // 에디터에서는 Awake가 돌지 않으므로, 기즈모와 검증 도구는 현재 배치로 그래프를 매번 새로 계산한다.
    public EnergyLineGraph CreateEditorGraph()
    {
        EnergyLineGraph graph = new EnergyLineGraph();
        graph.Build(transform, _startPoint, _endPoint, _pathNodes, _branches, _curveResolution, _splineTension);
        return graph;
    }
#endif

    private void InitPathNodes()
    {
        InitNodes(_pathNodes);
        for (int i = 0; i < _branches.Count; i++)
        {
            InitNodes(_branches[i].PathNodes);
        }
    }

    private void InitNodes(List<EnergyPathNode> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].Init();
        }
    }

    private void InitGraph()
    {
        _graph.Build(transform, _startPoint, _endPoint, _pathNodes, _branches, _curveResolution, _splineTension);
        for (int i = 0; i < _graph.InvalidBranchIndices.Count; i++)
        {
            Debug.LogWarning($"[EnergyLine] {name}의 {_graph.InvalidBranchIndices[i]}번 분기는 분기점이 앞선 경로 위에 없어 무시합니다.", this);
        }
        _nodeActivator.Init(_graph.Paths);
    }
}
