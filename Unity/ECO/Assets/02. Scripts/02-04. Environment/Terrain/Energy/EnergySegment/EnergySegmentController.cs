using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnergySegmentController
{
    private List<EnergySegment> _activeSegments = new List<EnergySegment>();
    private EnergyLineRendererDrawer _lineDrawer = new EnergyLineRendererDrawer();
    public IReadOnlyList<EnergySegment> ActiveSegments => _activeSegments;

    public void StartNewSegment(GameObject prefab, Transform parent)
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
            GameObject instance = Object.Instantiate(prefab, parent);
            instance.SetActive(true);
            newSegment.GameObjectInstance = instance;
            instance.GetComponentsInChildren<LineRenderer>(true, newSegment.ChildLineRenderers);
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

    // cutOffDelay 없이 즉시 꼬리 추적을 시작한다. 홀드 방식 스위치처럼 즉각적인 차단이 필요할 때 사용한다.
    public void StopCurrentSegmentImmediately()
    {
        if (_activeSegments.Count == 0)
        {
            return;
        }

        EnergySegment targetSegment = _activeSegments[_activeSegments.Count - 1];
        if (targetSegment.IsCuttingOff)
        {
            return;
        }

        targetSegment.IsWaitingToCutOff = false;
        targetSegment.IsCuttingOff = true;
    }

    public void UpdateSegments(float deltaTime, float totalDistance, float energySpeed)
    {
        for (int i = _activeSegments.Count - 1; 0<= i ; i--)
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

            if (totalDistance<= segment.TailDistance )
            {
                if (segment.GameObjectInstance != null)
                {
                    Object.Destroy(segment.GameObjectInstance);
                }
                
                _activeSegments.RemoveAt(i);
            }
        }
    }

    public void RenderSegments(List<Vector3> computedWaypoints)
    {
        foreach (EnergySegment segment in _activeSegments)
        {
            if (segment.ChildLineRenderers is not null)
            {
                foreach (var lr in segment.ChildLineRenderers)
                {
                    if (lr != null)
                    {
                        _lineDrawer.UpdateSegmentRenderer(lr, computedWaypoints, segment.TailDistance, segment.HeadDistance);
                    }
                }
            }
        }
    }
}
