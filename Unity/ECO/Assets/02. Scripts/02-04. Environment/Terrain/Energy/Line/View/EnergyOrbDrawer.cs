using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 펄스마다, 그 펄스의 머리가 지나는 중인 분기마다 에너지구를 하나씩 둔다.
/// 머리가 분기에 들어오면 풀에서 꺼내고 분기 끝에 닿으면 반납한다. 오버레이 드로어와 같은 슬롯 방식이다.
/// </summary>
public class EnergyOrbDrawer
{
    private List<EnergyOrbSlot> _slots = new List<EnergyOrbSlot>();
    private Stack<EnergyOrbSlot> _freeSlots = new Stack<EnergyOrbSlot>();

    public void Refresh(IReadOnlyList<EnergySegment> segments, IReadOnlyList<EnergyLineBranchPath> paths)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].IsUsed = false;
        }

        for (int s = 0; s < segments.Count; s++)
        {
            for (int b = 0; b < paths.Count; b++)
            {
                DrawOnBranch(segments[s], b, paths[b]);
            }
        }

        for (int i = _slots.Count - 1; 0 <= i; i--)
        {
            if (!_slots[i].IsUsed)
            {
                ReleaseSlot(i);
            }
        }
    }

    public void ReleaseAll()
    {
        for (int i = _slots.Count - 1; 0 <= i; i--)
        {
            ReleaseSlot(i);
        }
    }

    // 분기점에서는 부모 분기에만 두고, 머리가 분기점을 넘어선 뒤부터 자식 분기에도 둔다.
    // 가득 채운 선은 머리가 이미 모든 분기 끝에 있으므로 에너지구가 생기지 않는다.
    private void DrawOnBranch(EnergySegment segment, int branchIndex, EnergyLineBranchPath path)
    {
        float headDistance = segment.HeadDistance;
        if (headDistance <= path.StartDistance || path.EndDistance <= headDistance)
        {
            return;
        }

        EnergyOrbSlot slot = GetOrAcquireSlot(segment, branchIndex);
        if (slot is null)
        {
            return;
        }

        slot.IsUsed = true;
        slot.OrbTransform.position = path.SamplePosition(headDistance - path.StartDistance);
    }

    private EnergyOrbSlot GetOrAcquireSlot(EnergySegment segment, int branchIndex)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (ReferenceEquals(_slots[i].Segment, segment) && _slots[i].BranchIndex == branchIndex)
            {
                return _slots[i];
            }
        }

        // Managers 프리팹이 없는 씬(프리팹 단독 테스트 등)에서는 에너지구 없이 판정만 돈다.
        if (!PoolManager.HasInstance)
        {
            return null;
        }

        GameObject orb = PoolManager.Instance.Pop(EPoolable.EnergyOrb);
        if (orb == null)
        {
            return null;
        }

        EnergyOrbSlot slot = 0 < _freeSlots.Count ? _freeSlots.Pop() : new EnergyOrbSlot();
        slot.Segment = segment;
        slot.BranchIndex = branchIndex;
        slot.Orb = orb;
        slot.OrbTransform = orb.transform;
        _slots.Add(slot);
        return slot;
    }

    private void ReleaseSlot(int index)
    {
        EnergyOrbSlot slot = _slots[index];

        // 앱 종료 중에는 매니저나 에너지구가 먼저 파괴되어 반납할 곳이 없을 수 있다.
        if (PoolManager.HasInstance && slot.Orb != null)
        {
            PoolManager.Instance.Push(EPoolable.EnergyOrb, slot.Orb);
        }

        slot.Segment = null;
        slot.Orb = null;
        slot.OrbTransform = null;
        _slots.RemoveAt(index);
        _freeSlots.Push(slot);
    }
}
