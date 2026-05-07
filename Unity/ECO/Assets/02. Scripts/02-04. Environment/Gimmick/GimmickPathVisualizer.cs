using System.Collections.Generic;
using UnityEngine;

public class GimmickPathVisualizer
{
    private LineRenderer _prefab;
    private LineRenderer _lineRenderer;
    private Vector2 _startPosition;
    private List<Transform> _waypoints;

    public GimmickPathVisualizer(LineRenderer prefab, Vector2 startPosition, List<Transform> waypoints)
    {
        _prefab = prefab;
        _startPosition = startPosition;
        _waypoints = waypoints;
    }

    public void Show(Transform parent)
    {
        if (_lineRenderer != null)
        {
            return;
        }

        if (_prefab == null)
        {
            return;
        }

        _lineRenderer = Object.Instantiate(_prefab, parent);
        _lineRenderer.gameObject.SetActive(true);
        RefreshPositions();
    }

    public void Hide()
    {
        if (_lineRenderer == null)
        {
            return;
        }

        Object.Destroy(_lineRenderer.gameObject);
        _lineRenderer = null;
    }

    private void RefreshPositions()
    {
        if (_lineRenderer == null)
        {
            return;
        }

        if (_waypoints == null)
        {
            _lineRenderer.positionCount = 0;
            return;
        }

        int count = 1;
        for (int i = 0; i < _waypoints.Count; i++)
        {
            if (_waypoints[i] != null)
            {
                count++;
            }
        }

        _lineRenderer.positionCount = count;
        int index = 0;
        _lineRenderer.SetPosition(index, new Vector3(_startPosition.x, _startPosition.y, 0f));
        index++;

        for (int i = 0; i < _waypoints.Count; i++)
        {
            if (_waypoints[i] != null)
            {
                _lineRenderer.SetPosition(index, _waypoints[i].position);
                index++;
            }
        }
    }
}
