using System.Collections.Generic;

/// <summary>
/// (대상, 채널)마다 지금 누가 값을 쓰고 있는지 적어 두는 표.
/// 자리가 10개 안팎이라, 구조체 키 딕셔너리의 조회 박싱을 피해 선형 탐색을 쓴다.
/// </summary>
public class UI_ReactionChannelTable
{
    private readonly List<UI_ReactionChannelOwner> _owners = new List<UI_ReactionChannelOwner>();

    public int Count => _owners.Count;

    public UI_ReactionChannelOwner this[int index] => _owners[index];

    public bool TryGet(int targetId, EUIReactionChannel channel, out UI_ReactionChannelOwner owner)
    {
        int index = IndexOf(targetId, channel);
        if (index < 0)
        {
            owner = default;
            return false;
        }

        owner = _owners[index];
        return true;
    }

    /// <summary>같은 자리에 이미 주인이 있으면 갈아 끼운다.</summary>
    public void Set(in UI_ReactionChannelOwner owner)
    {
        int index = IndexOf(owner.TargetId, owner.Channel);
        if (0 <= index)
        {
            _owners[index] = owner;
            return;
        }

        _owners.Add(owner);
    }

    public void RemoveAt(int index)
    {
        _owners.RemoveAt(index);
    }

    /// <summary>지정한 리액션이 아직 그 자리의 주인일 때만 비운다.</summary>
    public void Release(int targetId, EUIReactionChannel channel, UI_ReactionBase reaction)
    {
        int index = IndexOf(targetId, channel);
        if (index < 0 || _owners[index].Reaction != reaction)
        {
            return;
        }

        _owners.RemoveAt(index);
    }

    public void Clear()
    {
        _owners.Clear();
    }

    private int IndexOf(int targetId, EUIReactionChannel channel)
    {
        for (int i = 0; i < _owners.Count; i++)
        {
            if (_owners[i].TargetId == targetId && _owners[i].Channel == channel)
            {
                return i;
            }
        }

        return -1;
    }
}
