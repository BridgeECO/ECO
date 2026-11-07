using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 펄스마다, 그 펄스가 걸친 분기마다 오버레이를 하나씩 붙여 에너지가 찬 구간을 그린다.
/// 오버레이 개수는 펄스가 생기거나 사라질 때만 바뀌므로 그때만 풀에서 꺼내고 반납한다.
/// </summary>
public class EnergyLineOverlayDrawer
{
    private List<EnergyOverlaySlot> _slots = new List<EnergyOverlaySlot>();
    private Stack<EnergyOverlaySlot> _freeSlots = new Stack<EnergyOverlaySlot>();
    private EnergyLineRendererDrawer _lineDrawer = new EnergyLineRendererDrawer();

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

    private void DrawOnBranch(EnergySegment segment, int branchIndex, EnergyLineBranchPath path)
    {
        float localStart = Mathf.Max(segment.TailDistance, path.StartDistance) - path.StartDistance;
        float localEnd = Mathf.Min(segment.HeadDistance, path.EndDistance) - path.StartDistance;
        if (localEnd <= localStart)
        {
            return;
        }

        EnergyOverlaySlot slot = GetOrAcquireSlot(segment, branchIndex);
        if (slot == null)
        {
            return;
        }

        slot.IsUsed = true;
        if (Mathf.Approximately(slot.DrawnStart, localStart) && Mathf.Approximately(slot.DrawnEnd, localEnd))
        {
            return;
        }

        slot.DrawnStart = localStart;
        slot.DrawnEnd = localEnd;
        for (int i = 0; i < slot.Renderers.Count; i++)
        {
            _lineDrawer.UpdateSegmentRenderer(slot.Renderers[i], path, localStart, localEnd);
        }
    }

    private EnergyOverlaySlot GetOrAcquireSlot(EnergySegment segment, int branchIndex)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (ReferenceEquals(_slots[i].Segment, segment) && _slots[i].BranchIndex == branchIndex)
            {
                return _slots[i];
            }
        }

        // Managers 프리팹이 없는 씬(프리팹 단독 테스트 등)에서는 오버레이 없이 판정만 돈다.
        if (!PoolManager.HasInstance)
        {
            return null;
        }

        GameObject overlay = PoolManager.Instance.Pop(EPoolable.EnergyLineOverlay);
        if (overlay == null)
        {
            return null;
        }

        EnergyOverlaySlot slot = 0 < _freeSlots.Count ? _freeSlots.Pop() : new EnergyOverlaySlot();
        slot.Segment = segment;
        slot.BranchIndex = branchIndex;
        slot.Overlay = overlay;
        slot.DrawnStart = -1f;
        slot.DrawnEnd = -1f;
        overlay.GetComponentsInChildren(true, slot.Renderers);
        _slots.Add(slot);
        return slot;
    }

    private void ReleaseSlot(int index)
    {
        EnergyOverlaySlot slot = _slots[index];

        // 앱 종료 중에는 매니저나 오버레이가 먼저 파괴되어 반납할 곳이 없을 수 있다.
        if (PoolManager.HasInstance && slot.Overlay != null)
        {
            PoolManager.Instance.Push(EPoolable.EnergyLineOverlay, slot.Overlay);
        }

        slot.Segment = null;
        slot.Overlay = null;
        slot.Renderers.Clear();
        _slots.RemoveAt(index);
        _freeSlots.Push(slot);
    }
}
