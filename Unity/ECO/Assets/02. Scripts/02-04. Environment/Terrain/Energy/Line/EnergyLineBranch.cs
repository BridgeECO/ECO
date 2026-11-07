using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 메인 경로나 앞선 분기 위의 한 점에서 갈라져 나가는 추가 경로.
/// 분기점은 이미 정의된 경로 위에만 둘 수 있어서 순환 구조가 생기지 않는다.
/// </summary>
[Serializable]
public class EnergyLineBranch
{
    [Tooltip("이 분기가 갈라지는 지점입니다. 메인 경로나 이 분기보다 앞에 있는 분기의 웨이포인트를 지정합니다.")]
    [SerializeField]
    private Transform _forkPoint;

    [SerializeField]
    private List<EnergyPathNode> _pathNodes = new List<EnergyPathNode>();

    [SerializeField]
    private Transform _endPoint;

    public Transform ForkPoint => _forkPoint;
    public List<EnergyPathNode> PathNodes => _pathNodes;
    public Transform EndPoint => _endPoint;
}
