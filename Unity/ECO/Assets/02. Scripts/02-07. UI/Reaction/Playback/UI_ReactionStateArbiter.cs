using System.Collections.Generic;
using UnityEngine;

/// <summary>지금 성립하는 상태들로부터 (대상, 채널)마다 이길 리액션 하나씩을 뽑는다.</summary>
public class UI_ReactionStateArbiter
{
    private readonly List<UI_ReactionChannelOwner> _candidates = new List<UI_ReactionChannelOwner>();

    public int Count => _candidates.Count;

    public UI_ReactionChannelOwner this[int index] => _candidates[index];

    /// <summary>
    /// Unity Selectable의 판정 순서를 그대로 따른다.
    /// 기획 의도상 Hover가 Selected를 이겨야 한다면 여기 순서만 바꾸면 된다.
    /// </summary>
    public static int GetStatePriority(EUIReactionState state)
    {
        switch (state)
        {
            case EUIReactionState.Disabled:
                return 4;

            case EUIReactionState.Pressed:
                return 3;

            case EUIReactionState.Selected:
                return 2;

            case EUIReactionState.Hover:
                return 1;

            default:
                return 0;
        }
    }

    public void Collect(IReadOnlyList<UI_ReactionEntry> entries, UI_ReactionStateTracker tracker,
        UI_ReactionContext context)
    {
        _candidates.Clear();

        for (int e = 0; e < entries.Count; e++)
        {
            UI_ReactionEntry entry = entries[e];
            if (entry.Kind != EUIReactionTriggerKind.State || !entry.IsEnabled)
            {
                continue;
            }

            if (!tracker.IsActive(entry.StateTrigger))
            {
                continue;
            }

            int priority = GetStatePriority(entry.StateTrigger);
            List<UI_ReactionBase> reactions = entry.Reactions;
            for (int r = 0; r < reactions.Count; r++)
            {
                Add(reactions[r], entry, priority, context);
            }
        }
    }

    public bool TryGet(int targetId, EUIReactionChannel channel, out UI_ReactionChannelOwner candidate)
    {
        for (int i = 0; i < _candidates.Count; i++)
        {
            if (_candidates[i].TargetId == targetId && _candidates[i].Channel == channel)
            {
                candidate = _candidates[i];
                return true;
            }
        }

        candidate = default;
        return false;
    }

    public void Clear()
    {
        _candidates.Clear();
    }

    private void Add(UI_ReactionBase reaction, UI_ReactionEntry entry, int priority, UI_ReactionContext context)
    {
        if (reaction == null)
        {
            return;
        }

        GameObject target = reaction.ResolveTarget(context);
        if (target == null)
        {
            return;
        }

        UI_ReactionChannelOwner candidate = new UI_ReactionChannelOwner
        {
            TargetId = target.GetInstanceID(),
            Channel = reaction.Channel,
            Reaction = reaction,
            Priority = priority,
            InterruptPolicy = entry.InterruptPolicy,
            ExitPolicy = entry.ExitPolicy,
        };

        for (int i = 0; i < _candidates.Count; i++)
        {
            if (_candidates[i].TargetId != candidate.TargetId || _candidates[i].Channel != candidate.Channel)
            {
                continue;
            }

            // 같은 순위면 먼저 등록된 항목이 이긴다. 목록 순서만으로 결과가 뒤집히지 않게 한다.
            if (_candidates[i].Priority < candidate.Priority)
            {
                _candidates[i] = candidate;
            }

            return;
        }

        _candidates.Add(candidate);
    }
}
