using System.Collections.Generic;
using UnityEngine;

public class EnergySegmentController
{
    private List<EnergySegment> _activeSegments = new List<EnergySegment>();

    // 스위치를 조작할 때마다 세그먼트가 생기므로, 다 쓴 인스턴스를 모아 두었다가 재사용한다.
    private Stack<EnergySegment> _segmentPool = new Stack<EnergySegment>();

    public IReadOnlyList<EnergySegment> ActiveSegments => _activeSegments;

    public void StartNewSegment()
    {
        if (0 < _activeSegments.Count && !_activeSegments[_activeSegments.Count - 1].IsCuttingOff)
        {
            return;
        }

        _activeSegments.Add(RentSegment(0f));
    }

    // 즉시 꼬리 추적을 시작한다. 홀드 방식 스위치처럼 즉각적인 차단이 필요할 때 사용한다.
    public void StopCurrentSegmentImmediately()
    {
        if (_activeSegments.Count == 0)
        {
            return;
        }

        _activeSegments[_activeSegments.Count - 1].IsCuttingOff = true;
    }

    // 처음부터 공급 중인 선을 흘려보내는 연출 없이 가득 찬 상태로 만든다.
    public void FillImmediately(float maxDistance)
    {
        ClearImmediately();
        _activeSegments.Add(RentSegment(maxDistance));
    }

    public void ClearImmediately()
    {
        for (int i = _activeSegments.Count - 1; 0 <= i; i--)
        {
            ReturnSegment(i);
        }
    }

    public void UpdateSegments(float deltaTime, float maxDistance, float energySpeed)
    {
        for (int i = _activeSegments.Count - 1; 0 <= i; i--)
        {
            EnergySegment segment = _activeSegments[i];

            if (segment.HeadDistance < maxDistance)
            {
                segment.HeadDistance = Mathf.Min(segment.HeadDistance + energySpeed * deltaTime, maxDistance);
            }

            if (segment.IsCuttingOff)
            {
                segment.TailDistance = Mathf.Min(segment.TailDistance + energySpeed * deltaTime, maxDistance);
            }

            if (maxDistance <= segment.TailDistance)
            {
                ReturnSegment(i);
            }
        }
    }

    private EnergySegment RentSegment(float headDistance)
    {
        EnergySegment segment = 0 < _segmentPool.Count ? _segmentPool.Pop() : new EnergySegment();
        segment.HeadDistance = headDistance;
        segment.TailDistance = 0f;
        segment.IsCuttingOff = false;
        return segment;
    }

    private void ReturnSegment(int index)
    {
        _segmentPool.Push(_activeSegments[index]);
        _activeSegments.RemoveAt(index);
    }
}
