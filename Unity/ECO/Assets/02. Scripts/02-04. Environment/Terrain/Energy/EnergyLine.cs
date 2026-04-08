using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

public class EnergyLine : MonoBehaviour
{
    [Foldout("Project")]
    [SerializeField]
    private List<Transform> _waypoints = new List<Transform>();

    [SerializeField]
    private LineRenderer _lineRendererPrefab;

    [SerializeField]
    private float _energySpeed = 2f;

    [SerializeField]
    private float _cutOffDelay = 3f;

    [SerializeField]
    private List<EnergyTerrainConnection> _connectedTerrains = new List<EnergyTerrainConnection>();

    private List<EnergySegment> _activeSegments = new List<EnergySegment>();
    private float _totalDistance;

    private void Awake()
    {
        CalculateTotalDistance();
    }

    private void CalculateTotalDistance()
    {
        _totalDistance = 0f;
        for (int i = 0; i < _waypoints.Count - 1; i++)
        {
            if (_waypoints[i] != null && _waypoints[i + 1] != null)
            {
                _totalDistance += Vector3.Distance(_waypoints[i].position, _waypoints[i + 1].position);
            }
        }
    }

    public void SetSwitchState(bool isOn)
    {
        if (isOn)
        {
            StartNewSegment();
        }
        else
        {
            StopCurrentSegmentAsync().Forget();
        }
    }

    private void StartNewSegment()
    {
        if (0 < _activeSegments.Count)
        {
            EnergySegment lastSegment = _activeSegments[_activeSegments.Count - 1];
            if (!lastSegment.IsCuttingOff && !lastSegment.IsWaitingToCutOff)
            {
                return;
            }
        }

        EnergySegment newSegment = new EnergySegment
        {
            HeadDistance = 0f,
            TailDistance = 0f,
            IsCuttingOff = false,
            IsWaitingToCutOff = false
        };

        if (_lineRendererPrefab != null)
        {
            newSegment.LineRendererInstance = Instantiate(_lineRendererPrefab, transform);
            newSegment.LineRendererInstance.gameObject.SetActive(true);
        }

        _activeSegments.Add(newSegment);
    }

    private async UniTaskVoid StopCurrentSegmentAsync()
    {
        if (_activeSegments.Count == 0)
        {
            return;
        }

        EnergySegment targetSegment = _activeSegments[_activeSegments.Count - 1];
        if (targetSegment.IsCuttingOff || targetSegment.IsWaitingToCutOff)
        {
            return;
        }

        targetSegment.IsWaitingToCutOff = true;
        await UniTask.Delay(System.TimeSpan.FromSeconds(_cutOffDelay));

        if (_activeSegments.Contains(targetSegment))
        {
            targetSegment.IsWaitingToCutOff = false;
            targetSegment.IsCuttingOff = true;
        }
    }

    private void Update()
    {
        UpdateSegments();
        UpdateTerrains();
        RenderSegments();
    }

    private void UpdateSegments()
    {
        for (int i = _activeSegments.Count - 1; i >= 0; i--)
        {
            EnergySegment segment = _activeSegments[i];

            if (segment.HeadDistance < _totalDistance)
            {
                segment.HeadDistance += _energySpeed * Time.deltaTime;
                segment.HeadDistance = Mathf.Min(segment.HeadDistance, _totalDistance);
            }

            if (segment.IsCuttingOff)
            {
                segment.TailDistance += _energySpeed * Time.deltaTime;
                segment.TailDistance = Mathf.Min(segment.TailDistance, _totalDistance);
            }

            if (segment.TailDistance >= _totalDistance)
            {
                if (segment.LineRendererInstance != null)
                {
                    Destroy(segment.LineRendererInstance.gameObject);
                }
                _activeSegments.RemoveAt(i);
            }
        }
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

            foreach (EnergySegment segment in _activeSegments)
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

    private void RenderSegments()
    {
        foreach (EnergySegment segment in _activeSegments)
        {
            if (segment.LineRendererInstance != null)
            {
                UpdateLineRenderer(segment.LineRendererInstance, segment.TailDistance, segment.HeadDistance);
            }
        }
    }

    private void UpdateLineRenderer(LineRenderer lr, float startDist, float endDist)
    {
        List<Vector3> points = new List<Vector3>();
        float currentDist = 0f;

        for (int i = 0; i < _waypoints.Count - 1; i++)
        {
            if (_waypoints[i] == null || _waypoints[i + 1] == null) continue;

            Vector3 p1 = _waypoints[i].position;
            Vector3 p2 = _waypoints[i + 1].position;
            float segLen = Vector3.Distance(p1, p2);

            float nextDist = currentDist + segLen;

            if (startDist <= nextDist && endDist >= currentDist)
            {
                float localStart = Mathf.Max(0, startDist - currentDist);
                float localEnd = Mathf.Min(segLen, endDist - currentDist);

                Vector3 pointStart = Vector3.Lerp(p1, p2, localStart / segLen);
                Vector3 pointEnd = Vector3.Lerp(p1, p2, localEnd / segLen);

                if (points.Count == 0 || points[points.Count - 1] != pointStart)
                {
                    points.Add(pointStart);
                }
                points.Add(pointEnd);
            }

            currentDist = nextDist;
        }

        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
    }
}
