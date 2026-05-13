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

    [SerializeField]
    private EnergyCore _energyCore;

    private Vector3 _staticActivationPosition;
    private Vector3 _staticDeactivationPosition;
    private bool _isCaptured;

    public EEnergyPathNodeType NodeType => _nodeType;

    #region Terrain
    public TerrainObject Terrain => _terrain;
    public Vector3 StaticActivationPosition => _staticActivationPosition;
    public Vector3 StaticDeactivationPosition => _staticDeactivationPosition;
    #endregion

    #region Waypoint
    public Transform Waypoint => _waypoint;
    #endregion

    #region EnergyCore
    public EnergyCore EnergyCore => _energyCore;
    #endregion

    public IEnergyReceiver EnergyReceiver { get; private set; }
    public bool IsCaptured => _isCaptured;

    public float ActivationCenterDistance { get; set; }
    public float DeactivationEndDistance { get; set; }
    public bool IsActiveInternal { get; set; }


    public void Init()
    {
        CaptureStaticPositions();
        AllocIEnergyReceiver();
    }

    private void CaptureStaticPositions()
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

    private void AllocIEnergyReceiver()
    {
        switch (_nodeType)
        {
            case EEnergyPathNodeType.EnergyCore:
                EnergyReceiver = _energyCore;
                break;
            case EEnergyPathNodeType.Terrain:
                EnergyReceiver = _terrain;
                break;
            default:
                break;
        }
    }
}
