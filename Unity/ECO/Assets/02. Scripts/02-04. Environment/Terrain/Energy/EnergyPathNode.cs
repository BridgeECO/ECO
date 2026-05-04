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

    private Vector3 _staticActivationPosition;
    private Vector3 _staticDeactivationPosition;
    private bool _isCaptured;

    public EEnergyPathNodeType NodeType => _nodeType;
    public TerrainObject Terrain => _terrain;
    public Transform Waypoint => _waypoint;
    public Vector3 StaticActivationPosition => _staticActivationPosition;
    public Vector3 StaticDeactivationPosition => _staticDeactivationPosition;
    public bool IsCaptured => _isCaptured;

    public float ActivationCenterDistance { get; set; }
    public float DeactivationEndDistance { get; set; }
    public bool IsActiveInternal { get; set; }

    public void CaptureStaticPositions()
    {
        if (_terrain == null || !_terrain.HasMovementGimmick)
        {
            return;
        }

        _staticActivationPosition = _terrain.ActivationPosition != null
            ? _terrain.ActivationPosition.position
            : _terrain.transform.position;

        _staticDeactivationPosition = _terrain.DeactivationPosition != null
            ? _terrain.DeactivationPosition.position
            : _terrain.transform.position;

        _isCaptured = true;
    }
}
