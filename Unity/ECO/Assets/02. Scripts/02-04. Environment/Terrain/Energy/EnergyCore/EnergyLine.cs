using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class EnergyLine : MonoBehaviour
{
    [Foldout("Project")]
    [SerializeField]
    private GameObject _lineRendererPrefab;

    [SerializeField]
    private Transform _startPoint;

    [SerializeField]
    private Transform _endPoint;

    [Tooltip("에너지 라인 곡선의 분할 해상도 (적정값 : 0 ~ 20)")]
    [SerializeField]
    private int _curveResolution;

    [Tooltip("스위치 최초 발동 시 에너지 구를 카메라가 추적하는 기능 활성화 여부")]
    [SerializeField]
    private bool _useTracking = false;

    [Tooltip("에너지 조각의 이동 속도")]
    [SerializeField]
    private float _energySpeed;

    [Tooltip("스플라인 곡선 경로의 장력 및 휘어짐 강도 (적정값: 0.0 ~ 1.2)")]
    [SerializeField]
    private float _splineTension;

    [SerializeField]
    private List<EnergyPathNode> _pathNodes = new List<EnergyPathNode>();

    private List<Vector3> _computedWaypoints = new List<Vector3>();
    private float _totalDistance;

    private EnergyPathCalculator _pathCalculator = new EnergyPathCalculator();
    private EnergySegmentController _segmentController = new EnergySegmentController();

    public bool UseTracking => _useTracking;
    public float TotalDistance => _totalDistance;

    private void Awake()
    {
        InitPathNodes();
        InitPathAndDistances();
    }

    private void Update()
    {
        _segmentController.UpdateSegments(Time.deltaTime, _totalDistance, _energySpeed);
        _segmentController.RenderSegments(_computedWaypoints);
        UpdatePathNodes();
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        List<Vector3> keyPoints = _pathCalculator.GetKeyPoints(_startPoint, _endPoint, transform, _pathNodes);
        if (keyPoints.Count < 2)
        {
            return;
        }

        List<Vector3> editorWaypoints;
        float editorDistance;
        _pathCalculator.CalculatePathAndDistances(
            _startPoint,
            _endPoint,
            transform,
            _pathNodes,
            _curveResolution,
            _splineTension,
            out editorWaypoints,
            out editorDistance
        );

        Gizmos.color = Color.cyan;
        for (int i = 0; i < editorWaypoints.Count - 1; i++)
        {
            Gizmos.DrawLine(editorWaypoints[i], editorWaypoints[i + 1]);
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < keyPoints.Count; i++)
        {
            Gizmos.DrawSphere(keyPoints[i], 0.1f);
        }
#endif
    }

    private void InitPathNodes()
    {
        foreach (var node in _pathNodes)
        {
            node.Init();
        }
    }

    private void InitPathAndDistances()
    {
        _pathCalculator.CalculatePathAndDistances(
            _startPoint,
            _endPoint,
            transform,
            _pathNodes,
            _curveResolution,
            _splineTension,
            out _computedWaypoints,
            out _totalDistance
        );
    }

    private void UpdatePathNodes()
    {
        foreach (var node in _pathNodes)
        {
            if (node.NodeType == EEnergyPathNodeType.Waypoint)
            {
                continue;
            }

            bool isActive = IsAnySegmentActive(node);
            if (node.IsActiveInternal == isActive)
            {
                continue;
            }
            node.IsActiveInternal = isActive;
            ApplyNodeActive(node, isActive);
        }
    }

    private bool IsAnySegmentActive(EnergyPathNode node)
    {
        foreach (EnergySegment segment in _segmentController.ActiveSegments)
        {
            if (node.ActivationCenterDistance <= segment.HeadDistance &&
                segment.TailDistance < node.DeactivationEndDistance)
            {
                return true;
            }
        }
        return false;
    }

    private void ApplyNodeActive(EnergyPathNode node, bool isActive)
    {
        if (node.EnergyReceiver == null)
        {
            return;
        }
        node.EnergyReceiver.SetEnergyActive(isActive);
    }

    public void SetSwitchState(bool isOn)
    {
        if (isOn)
        {
            _segmentController.StartNewSegment(_lineRendererPrefab, transform);
            return;
        }
        _segmentController.StopCurrentSegmentImmediately();
    }

    // HeadDistance를 _computedWaypoints 상에서 선형 보간하여 현재 에너지 구의 월드 포지션을 반환한다.
    public bool TryGetSegmentHeadWorldPosition(out Vector3 worldPos)
    {
        IReadOnlyList<EnergySegment> segments = _segmentController.ActiveSegments;
        if (segments.Count == 0 || _computedWaypoints.Count < 2)
        {
            worldPos = Vector3.zero;
            return false;
        }

        float headDistance = segments[segments.Count - 1].HeadDistance;
        worldPos = SampleWorldPositionAtDistance(headDistance);
        return true;
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


    // 누적 거리 배열을 순회하여 목표 거리에 해당하는 월드 포지션을 선형 보간으로 계산한다.
    private Vector3 SampleWorldPositionAtDistance(float targetDistance)
    {
        float accumulated = 0f;
        for (int i = 0; i < _computedWaypoints.Count - 1; i++)
        {
            Vector3 p1 = _computedWaypoints[i];
            Vector3 p2 = _computedWaypoints[i + 1];
            float segmentLength = Vector3.Distance(p1, p2);
            float next = accumulated + segmentLength;

            if (targetDistance <= next)
            {
                float t = segmentLength > 0f ? (targetDistance - accumulated) / segmentLength : 0f;
                return Vector3.Lerp(p1, p2, t);
            }

            accumulated = next;
        }

        return _computedWaypoints[_computedWaypoints.Count - 1];
    }
}
