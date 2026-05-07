using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

public class EnergyLine : MonoBehaviour
{
    [Foldout("Project")]
    [SerializeField]
    private LineRenderer _lineRendererPrefab;

    [SerializeField]
    private Transform _startPoint;

    [SerializeField]
    private Transform _endPoint;

    [Tooltip("에너지 라인 곡선의 분할 해상도 (적정값 : 0 ~ 20)")]
    [SerializeField]
    private int _curveResolution;

    [Tooltip("에너지 조각의 이동 속도")]
    [SerializeField]
    private float _energySpeed;

    [Tooltip("에너지 차단 후 꼬리가 출발지점에서 떨어지기까지의 지연 시간, 값이 클수록 에너지 조각이 길어짐.")]
    [SerializeField]
    private float _cutOffDelay;

    [Tooltip("스플라인 곡선 경로의 장력 및 휘어짐 강도 (적정값: 0.0 ~ 1.2)")]
    [SerializeField]
    private float _splineTension;

    [SerializeField]
    private List<EnergyPathNode> _pathNodes = new List<EnergyPathNode>();

    private List<Vector3> _computedWaypoints = new List<Vector3>();
    private float _totalDistance;

    private EnergyPathCalculator _pathCalculator = new EnergyPathCalculator();
    private EnergySegmentController _segmentController = new EnergySegmentController();

    private void Awake()
    {
        CaptureMovementGimmickPositions();
        InitPathAndDistances();
    }

    private void Update()
    {
        _segmentController.UpdateSegments(Time.deltaTime, _totalDistance, _energySpeed);
        UpdateTerrains();
        _segmentController.RenderSegments(_computedWaypoints);
    }

    private void CaptureMovementGimmickPositions()
    {
        for (int i = 0; i < _pathNodes.Count; i++)
        {
            _pathNodes[i].CaptureStaticPositions();
        }
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

    public void SetSwitchState(bool isOn)
    {
        if (isOn)
        {
            _segmentController.StartNewSegment(_lineRendererPrefab, transform);
            return;
        }
        _segmentController.StopCurrentSegmentAsync(_cutOffDelay).Forget();
    }

    private void UpdateTerrains()
    {
        foreach (EnergyPathNode node in _pathNodes)
        {
            if (node.NodeType != EEnergyPathNodeType.Terrain)
            {
                continue;
            }

            if (node.Terrain == null)
            {
                continue;
            }

            bool isActive = false;
            foreach (EnergySegment segment in _segmentController.ActiveSegments)
            {
                if (segment.HeadDistance >= node.ActivationCenterDistance &&
                    segment.TailDistance < node.DeactivationEndDistance)
                {
                    isActive = true;
                    break;
                }
            }

            if (node.IsActiveInternal != isActive)
            {
                node.IsActiveInternal = isActive;
                node.Terrain.SetEnergyActive(isActive);
            }
        }
    }
}
