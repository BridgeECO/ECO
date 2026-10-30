using UnityEngine;

/// <summary>
/// 펄스 하나가 분기 하나 위에 두는 에너지구. 풀에서 꺼낸 오브젝트를 기억한다.
/// </summary>
public class EnergyOrbSlot
{
    public EnergySegment Segment { get; set; }
    public int BranchIndex { get; set; }
    public GameObject Orb { get; set; }
    public Transform OrbTransform { get; set; }
    public bool IsUsed { get; set; }
}
