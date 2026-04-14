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
    private List<EnergyTerrainConnection> _connectedTerrains = new List<EnergyTerrainConnection>();

    private List<Vector3> _computedWaypoints = new List<Vector3>();
    private float _totalDistance;

    private EnergyPathCalculator _pathCalculator = new EnergyPathCalculator();
    private EnergySegmentController _segmentController = new EnergySegmentController();

    private void Awake()
    {
        InitPathAndDistances();
    }

    private void InitPathAndDistances()
    {
        _pathCalculator.CalculatePathAndDistances(
            _startPoint,
            _endPoint,
            transform,
            _connectedTerrains,
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

    private void Update()
    {
        _segmentController.UpdateSegments(Time.deltaTime, _totalDistance, _energySpeed);
        UpdateTerrains();
        _segmentController.RenderSegments(_computedWaypoints);
    }

    private void UpdateTerrains()
    {
        foreach (EnergyTerrainConnection connection in _connectedTerrains)
        {
            if (connection.Terrain == null)
            {
                continue;
            }

            bool shouldBeActive = false;

            foreach (EnergySegment segment in _segmentController.ActiveSegments)
            {
                if (segment.HeadDistance >= connection.ActivationCenterDistance &&
                    segment.TailDistance < connection.DeactivationEndDistance)
                {
                    shouldBeActive = true;
                    break;
                }
            }

            if (connection.IsActiveInternal != shouldBeActive)
            {
                connection.IsActiveInternal = shouldBeActive;
                connection.Terrain.SetEnergyActive(shouldBeActive);
            }
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        InitPathAndDistances();

        if (_computedWaypoints == null || _computedWaypoints.Count < 2)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        for (int i = 0; i < _computedWaypoints.Count - 1; i++)
        {
            Gizmos.DrawLine(_computedWaypoints[i], _computedWaypoints[i + 1]);
        }

        Gizmos.color = Color.yellow;
        foreach (Vector3 waypoint in _pathCalculator.GetKeyPoints(_startPoint, _endPoint, transform, _connectedTerrains))
        {
            Gizmos.DrawSphere(waypoint, 0.1f);
        }
#endif
    }
}
