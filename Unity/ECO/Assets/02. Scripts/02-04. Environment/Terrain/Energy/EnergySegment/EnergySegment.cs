using UnityEngine;

public class EnergySegment
{
    public float HeadDistance { get; set; }
    public float TailDistance { get; set; }
    public bool IsCuttingOff { get; set; }
    public bool IsWaitingToCutOff { get; set; }
    public LineRenderer LineRendererInstance { get; set; }
}
