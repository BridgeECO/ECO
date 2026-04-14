using System;
using UnityEngine;

[Serializable]
public class EnergyTerrainConnection
{
    [SerializeField]
    private TerrainObject _terrain;
    public TerrainObject Terrain => _terrain;

    public float ActivationCenterDistance { get; set; }

    public float DeactivationEndDistance { get; set; }

    public bool IsActiveInternal { get; set; }
}
