using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnergySegmentController
{
    private List<EnergySegment> _activeSegments = new List<EnergySegment>();
    private EnergyLineRendererDrawer _lineDrawer = new EnergyLineRendererDrawer();
    public IReadOnlyList<EnergySegment> ActiveSegments => _activeSegments;

    public void StartNewSegment(LineRenderer prefab, Transform parent)
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

        if (prefab != null)
        {
            newSegment.LineRendererInstance = Object.Instantiate(prefab, parent);
            newSegment.LineRendererInstance.gameObject.SetActive(true);
        }

        _activeSegments.Add(newSegment);
    }

    public async UniTaskVoid StopCurrentSegmentAsync(float cutOffDelay)
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
        await UniTask.Delay(System.TimeSpan.FromSeconds(cutOffDelay));

        if (_activeSegments.Contains(targetSegment))
        {
            targetSegment.IsWaitingToCutOff = false;
            targetSegment.IsCuttingOff = true;
        }
    }

    public void UpdateSegments(float deltaTime, float totalDistance, float energySpeed)
    {
        for (int i = _activeSegments.Count - 1; i >= 0; i--)
        {
            EnergySegment segment = _activeSegments[i];

            if (segment.HeadDistance < totalDistance)
            {
                segment.HeadDistance += energySpeed * deltaTime;
                segment.HeadDistance = Mathf.Min(segment.HeadDistance, totalDistance);
            }

            if (segment.IsCuttingOff)
            {
                segment.TailDistance += energySpeed * deltaTime;
                segment.TailDistance = Mathf.Min(segment.TailDistance, totalDistance);
            }

            if (segment.TailDistance >= totalDistance)
            {
                if (segment.LineRendererInstance != null)
                {
                    Object.Destroy(segment.LineRendererInstance.gameObject);
                }
                
                _activeSegments.RemoveAt(i);
            }
        }
    }

    public void RenderSegments(List<Vector3> computedWaypoints)
    {
        foreach (EnergySegment segment in _activeSegments)
        {
            if (segment.LineRendererInstance != null)
            {
                _lineDrawer.UpdateSegmentRenderer(segment.LineRendererInstance, computedWaypoints, segment.TailDistance, segment.HeadDistance);
            }
        }
    }
}
