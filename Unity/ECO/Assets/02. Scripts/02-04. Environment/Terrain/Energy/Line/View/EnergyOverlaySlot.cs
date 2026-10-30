using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 펄스 하나가 분기 하나 위에 그리는 오버레이. 풀에서 꺼낸 오브젝트와 마지막으로 그린 구간을 기억한다.
/// </summary>
public class EnergyOverlaySlot
{
    public EnergySegment Segment { get; set; }
    public int BranchIndex { get; set; }
    public GameObject Overlay { get; set; }
    public List<LineRenderer> Renderers { get; } = new List<LineRenderer>();
    public float DrawnStart { get; set; }
    public float DrawnEnd { get; set; }
    public bool IsUsed { get; set; }
}
