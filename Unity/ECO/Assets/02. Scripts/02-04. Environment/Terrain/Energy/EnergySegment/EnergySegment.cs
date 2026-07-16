using System.Collections.Generic;
using UnityEngine;

public class EnergySegment
{
    public float HeadDistance { get; set; }
    public float TailDistance { get; set; }
    public bool IsCuttingOff { get; set; }
    public bool IsWaitingToCutOff { get; set; }
    public GameObject GameObjectInstance { get; set; }
    public List<LineRenderer> ChildLineRenderers { get; set; } = new List<LineRenderer>();
}
