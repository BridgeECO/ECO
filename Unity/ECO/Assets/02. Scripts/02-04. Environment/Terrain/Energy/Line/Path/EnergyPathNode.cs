using System;
using UnityEngine;

[Serializable]
public class EnergyPathNode
{
    [SerializeField]
    private EEnergyPathNodeType _nodeType = EEnergyPathNodeType.Waypoint;

    [SerializeField]
    private Transform _waypoint;

    [SerializeField]
    private EnergyCore _energyCore;

    public EEnergyPathNodeType NodeType => _nodeType;

    #region Waypoint
    public Transform Waypoint => _waypoint;
    #endregion

    #region EnergyCore
    public EnergyCore EnergyCore => _energyCore;
    #endregion

    public IEnergyReceiver EnergyReceiver { get; private set; }

    public float ActivationCenterDistance { get; set; }
    public float DeactivationEndDistance { get; set; }
    public bool IsActiveInternal { get; set; }


    public void Init()
    {
        AllocIEnergyReceiver();
    }

    private void AllocIEnergyReceiver()
    {
        if (_nodeType == EEnergyPathNodeType.EnergyCore)
        {
            EnergyReceiver = _energyCore;
        }
    }
}
