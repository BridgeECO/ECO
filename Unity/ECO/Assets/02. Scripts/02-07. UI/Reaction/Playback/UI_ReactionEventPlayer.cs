using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 단발 이벤트 리액션을 재생한다. 재생 동안 해당 자리를 뺏고 끝나면 돌려준다.
/// 뺏지 않으면 클릭 연출과 hover 연출이 같은 값을 매 프레임 번갈아 써 떨린다.
/// </summary>
public class UI_ReactionEventPlayer
{
    public const int EVENT_PRIORITY = int.MaxValue;

    private readonly UI_ReactionChannelTable _owners;
    private readonly Action _onChannelReleased;

    public UI_ReactionEventPlayer(UI_ReactionChannelTable owners, Action onChannelReleased)
    {
        _owners = owners;
        _onChannelReleased = onChannelReleased;
    }

    public void Play(IReadOnlyList<UI_ReactionEntry> entries, EUIReactionEvent uiEvent,
        UI_ReactionContext context, CancellationToken token)
    {
        for (int e = 0; e < entries.Count; e++)
        {
            UI_ReactionEntry entry = entries[e];
            if (!entry.MatchesEvent(uiEvent))
            {
                continue;
            }

            List<UI_ReactionBase> reactions = entry.Reactions;
            for (int r = 0; r < reactions.Count; r++)
            {
                PlayReactionAsync(reactions[r], entry, context, token).Forget();
            }
        }
    }

    private async UniTaskVoid PlayReactionAsync(UI_ReactionBase reaction, UI_ReactionEntry entry,
        UI_ReactionContext context, CancellationToken token)
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

        int targetId = target.GetInstanceID();
        EUIReactionChannel channel = reaction.Channel;

        if (_owners.TryGet(targetId, channel, out UI_ReactionChannelOwner previous)
            && previous.Reaction != reaction)
        {
            previous.Reaction.Kill();
        }

        _owners.Set(new UI_ReactionChannelOwner
        {
            TargetId = targetId,
            Channel = channel,
            Reaction = reaction,
            Priority = EVENT_PRIORITY,
            InterruptPolicy = entry.InterruptPolicy,
            ExitPolicy = entry.ExitPolicy,
        });

        await reaction.PlayAsync(context, entry.InterruptPolicy, token);

        _owners.Release(targetId, channel, reaction);
        _onChannelReleased?.Invoke();
    }
}
