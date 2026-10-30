using System.Collections.Generic;

/// <summary>
/// 선 위의 코어(와 레거시 지형 노드)를 에너지가 흐르는 구간에 맞춰 켜고 끈다.
/// </summary>
public class EnergyNodeActivator
{
    private List<EnergyPathNode> _receiverNodes = new List<EnergyPathNode>();

    public void Init(IReadOnlyList<EnergyLineBranchPath> paths)
    {
        _receiverNodes.Clear();

        for (int i = 0; i < paths.Count; i++)
        {
            IReadOnlyList<EnergyPathNode> nodes = paths[i].Nodes;
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].EnergyReceiver != null)
                {
                    _receiverNodes.Add(nodes[j]);
                }
            }
        }
    }

    public void Refresh(IReadOnlyList<EnergySegment> segments)
    {
        for (int i = 0; i < _receiverNodes.Count; i++)
        {
            EnergyPathNode node = _receiverNodes[i];
            bool isActive = IsAnySegmentActive(node, segments);
            if (node.IsActiveInternal == isActive)
            {
                continue;
            }

            node.IsActiveInternal = isActive;
            node.EnergyReceiver.SetEnergyActive(isActive);
        }
    }

    public void DeactivateAll()
    {
        for (int i = 0; i < _receiverNodes.Count; i++)
        {
            EnergyPathNode node = _receiverNodes[i];
            if (!node.IsActiveInternal)
            {
                continue;
            }

            node.IsActiveInternal = false;
            node.EnergyReceiver.SetEnergyActive(false);
        }
    }

    // 머리가 코어 중심에 닿으면 켜지고, 꼬리가 출구를 지나 에너지가 완전히 빠져나가면 꺼진다.
    // 매 프레임 호출되므로 인터페이스 열거자가 박싱되지 않게 인덱스로 순회한다.
    private bool IsAnySegmentActive(EnergyPathNode node, IReadOnlyList<EnergySegment> segments)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            EnergySegment segment = segments[i];
            if (node.ActivationCenterDistance <= segment.HeadDistance &&
                segment.TailDistance < node.DeactivationEndDistance)
            {
                return true;
            }
        }
        return false;
    }
}
