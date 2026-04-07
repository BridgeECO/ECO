using System;
using UnityEngine;

[Serializable]
public class EnergyTerrainConnection
{
    [SerializeField]
    private TerrainObject _terrain;
    public TerrainObject Terrain => _terrain;

    [SerializeField]
    private float _activationCenterDistance;
    public float ActivationCenterDistance => _activationCenterDistance;

    [SerializeField]
    private float _deactivationEndDistance;
    public float DeactivationEndDistance => _deactivationEndDistance;

    public bool IsActiveInternal { get; set; }
}
