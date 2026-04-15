using System;
using UnityEngine;

[Serializable]
public class EnergyPathNode
{
    [SerializeField]
    private EEnergyPathNodeType _nodeType;

    [SerializeField]
    private TerrainObject _terrain;

    [SerializeField]
    private Transform _waypoint;

    public EEnergyPathNodeType NodeType => _nodeType;
    public TerrainObject Terrain => _terrain;
    public Transform Waypoint => _waypoint;

    public float ActivationCenterDistance { get; set; }
    public float DeactivationEndDistance { get; set; }
    public bool IsActiveInternal { get; set; }
}
