using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 메인 경로와 분기를 곡선으로 계산하고, 경로 위 노드마다 에너지원에서부터의 거리를 매긴다.
/// 에너지는 모든 분기에서 같은 속도로 나아가므로, 이 거리 하나로 선 전체의 상태를 판정할 수 있다.
/// </summary>
public class EnergyLineGraph
{
    // 분기점은 부모 곡선의 키포인트와 같은 위치라 부동소수 오차만 허용한다.
    private const float FORK_TOLERANCE = 0.05f;

    // 해상도 0은 곡선 점을 하나도 만들지 않아 선이 사라지므로, 최소 직선 한 구간으로 본다.
    private const int MIN_CURVE_RESOLUTION = 1;

    private const float COINCIDENT_EXIT_EPSILON = 0.001f;

    private EnergyPathCalculator _pathCalculator = new EnergyPathCalculator();
    private List<EnergyLineBranchPath> _paths = new List<EnergyLineBranchPath>();
    private List<int> _longestChain = new List<int>();
    private List<int> _invalidBranchIndices = new List<int>();

    public IReadOnlyList<EnergyLineBranchPath> Paths => _paths;
    public IReadOnlyList<int> InvalidBranchIndices => _invalidBranchIndices;
    public float MaxDistance { get; private set; }

    public void Build(Transform owner, Transform startPoint, Transform endPoint, List<EnergyPathNode> mainNodes,
        List<EnergyLineBranch> branches, int curveResolution, float splineTension)
    {
        _paths.Clear();
        _invalidBranchIndices.Clear();
        int resolution = Mathf.Max(MIN_CURVE_RESOLUTION, curveResolution);

        AddPath(owner, startPoint, endPoint, mainNodes, 0f, -1, resolution, splineTension);

        for (int i = 0; i < branches.Count; i++)
        {
            EnergyLineBranch branch = branches[i];
            if (!TryFindFork(branch.ForkPoint, out int parentIndex, out float forkDistance))
            {
                _invalidBranchIndices.Add(i);
                continue;
            }

            AddPath(owner, branch.ForkPoint, branch.EndPoint, branch.PathNodes, forkDistance, parentIndex, resolution, splineTension);
        }

        InitLongestChain();
    }

    // 카메라 추적처럼 에너지구 하나만 따라가야 할 때는 가장 긴 경로 위의 머리를 쓴다.
    public bool TrySampleOnLongestPath(float distance, out Vector3 position)
    {
        for (int i = _longestChain.Count - 1; 0 <= i; i--)
        {
            EnergyLineBranchPath path = _paths[_longestChain[i]];
            if (path.StartDistance <= distance)
            {
                position = path.SamplePosition(distance - path.StartDistance);
                return true;
            }
        }

        position = Vector3.zero;
        return false;
    }

    private void AddPath(Transform owner, Transform startPoint, Transform endPoint, List<EnergyPathNode> nodes,
        float startDistance, int parentIndex, int resolution, float splineTension)
    {
        _pathCalculator.CalculatePathAndDistances(startPoint, endPoint, owner, nodes, resolution, splineTension,
            out List<Vector3> waypoints, out float _);
        InitNodeDistances(nodes, startDistance);
        _paths.Add(new EnergyLineBranchPath(waypoints, nodes, startDistance, parentIndex));
    }

    private bool TryFindFork(Transform forkPoint, out int parentIndex, out float forkDistance)
    {
        parentIndex = -1;
        forkDistance = 0f;
        if (forkPoint == null)
        {
            return false;
        }

        for (int i = 0; i < _paths.Count; i++)
        {
            if (_paths[i].TryFindLocalDistance(forkPoint.position, FORK_TOLERANCE, out float localDistance))
            {
                parentIndex = i;
                forkDistance = _paths[i].StartDistance + localDistance;
                return true;
            }
        }

        return false;
    }

    private void InitNodeDistances(List<EnergyPathNode> nodes, float startDistance)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            EnergyPathNode node = nodes[i];
            if (!IsReceiverNode(node))
            {
                continue;
            }

            node.ActivationCenterDistance += startDistance;
            node.DeactivationEndDistance += startDistance;

            // 출구 지점을 따로 옮겨 두지 않은 코어는 두 판정 지점이 같은 거리에 놓인다.
            // 이때는 코어 크기만큼 더 지나야 에너지가 완전히 빠져나간 것으로 본다.
            if (node.NodeType == EEnergyPathNodeType.EnergyCore &&
                node.DeactivationEndDistance - node.ActivationCenterDistance < COINCIDENT_EXIT_EPSILON)
            {
                node.DeactivationEndDistance = node.ActivationCenterDistance + node.EnergyCore.ExitRadius;
            }
        }
    }

    // 경로 계산기가 거리를 매기는 노드와 같은 조건이다.
    private bool IsReceiverNode(EnergyPathNode node)
    {
        return (node.NodeType == EEnergyPathNodeType.Terrain && node.Terrain != null) ||
               (node.NodeType == EEnergyPathNodeType.EnergyCore && node.EnergyCore != null);
    }

    private void InitLongestChain()
    {
        _longestChain.Clear();
        MaxDistance = 0f;
        int leafIndex = -1;

        for (int i = 0; i < _paths.Count; i++)
        {
            if (MaxDistance < _paths[i].EndDistance)
            {
                MaxDistance = _paths[i].EndDistance;
                leafIndex = i;
            }
        }

        for (int i = leafIndex; 0 <= i; i = _paths[i].ParentIndex)
        {
            _longestChain.Add(i);
        }
        _longestChain.Reverse();
    }
}
