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

    public EEnergyPathNodeType NodeType => ToDefinedType(_nodeType);

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


    // 0은 지운 지형 노드(Terrain)의 값이다. 인스펙터에서 빈 목록에 처음 추가한 항목도 필드 초기값 대신 이 값으로 생긴다.
    // 이런 정의되지 않은 값은 경로에서 빠지지 않도록 웨이포인트로 본다.
    public static EEnergyPathNodeType ToDefinedType(EEnergyPathNodeType nodeType)
    {
        return nodeType == EEnergyPathNodeType.EnergyCore ? EEnergyPathNodeType.EnergyCore : EEnergyPathNodeType.Waypoint;
    }

    public void Init()
    {
        AllocIEnergyReceiver();
    }

    private void AllocIEnergyReceiver()
    {
        if (NodeType == EEnergyPathNodeType.EnergyCore)
        {
            EnergyReceiver = _energyCore;
        }
    }
}
