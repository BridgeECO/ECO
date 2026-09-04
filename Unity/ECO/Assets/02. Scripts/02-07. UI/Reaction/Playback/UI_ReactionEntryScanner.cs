using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>항목 목록을 통째로 훑는 일만 모아 둔다. 상태를 갖지 않아 전부 정적 메서드다.</summary>
public static class UI_ReactionEntryScanner
{
    public static bool HasState(IReadOnlyList<UI_ReactionEntry> entries, EUIReactionState state)
    {
        if (entries == null)
        {
            return false;
        }

        for (int e = 0; e < entries.Count; e++)
        {
            if (entries[e].Kind == EUIReactionTriggerKind.State && entries[e].StateTrigger == state)
            {
                return true;
            }
        }

        return false;
    }

    public static void KillAll(IReadOnlyList<UI_ReactionEntry> entries)
    {
        if (entries == null)
        {
            return;
        }

        for (int e = 0; e < entries.Count; e++)
        {
            List<UI_ReactionBase> reactions = entries[e].Reactions;
            for (int r = 0; r < reactions.Count; r++)
            {
                reactions[r]?.Kill();
            }
        }
    }

    /// <summary>해당 신호에 묶인 리액션만 끊는다. 함께 돌고 있을 상태 연출까지 건드리지 않기 위해서다.</summary>
    public static void KillSignal(IReadOnlyList<UI_ReactionEntry> entries, EUIReactionSignal signal)
    {
        if (entries == null)
        {
            return;
        }

        for (int e = 0; e < entries.Count; e++)
        {
            UI_ReactionEntry entry = entries[e];
            if (!entry.MatchesSignal(signal))
            {
                continue;
            }

            List<UI_ReactionBase> reactions = entry.Reactions;
            for (int r = 0; r < reactions.Count; r++)
            {
                reactions[r]?.Kill();
            }
        }
    }

    /// <summary>재생 중인 리액션이 하나라도 있는지. 신호 재생은 자리 장부에 오르지 않아 따로 본다.</summary>
    public static bool IsAnyPlaying(IReadOnlyList<UI_ReactionEntry> entries)
    {
        if (entries == null)
        {
            return false;
        }

        for (int e = 0; e < entries.Count; e++)
        {
            List<UI_ReactionBase> reactions = entries[e].Reactions;
            for (int r = 0; r < reactions.Count; r++)
            {
                if (reactions[r] != null && reactions[r].IsPlaying)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static void RestoreAll(IReadOnlyList<UI_ReactionEntry> entries, UI_ReactionContext context)
    {
        if (entries == null)
        {
            return;
        }

        for (int e = 0; e < entries.Count; e++)
        {
            List<UI_ReactionBase> reactions = entries[e].Reactions;
            for (int r = 0; r < reactions.Count; r++)
            {
                reactions[r]?.RestoreBaseline(context);
            }
        }
    }

    public static void CollectSignalTasks(IReadOnlyList<UI_ReactionEntry> entries, EUIReactionSignal signal,
        UI_ReactionContext context, CancellationToken token, UI_ReactionDispatcher dispatcher, List<UniTask> results)
    {
        for (int e = 0; e < entries.Count; e++)
        {
            UI_ReactionEntry entry = entries[e];
            if (!entry.MatchesSignal(signal))
            {
                continue;
            }

            List<UI_ReactionBase> reactions = entry.Reactions;
            for (int r = 0; r < reactions.Count; r++)
            {
                UI_ReactionBase reaction = reactions[r];
                if (reaction == null)
                {
                    continue;
                }

                // 상태 연출이 쥐고 있던 자리를 신호가 가져간다.
                dispatcher.KillChannel(reaction, context);
                results.Add(reaction.PlayAsync(context, entry.InterruptPolicy, token));
            }
        }
    }

    /// <summary>재생해 둔 신호를 항목의 복귀 정책대로 되감는다.</summary>
    // 물러나는 쪽은 자리를 새로 잡지 않으므로 KillChannel을 부르지 않는다. 상태 연출이 쥔 자리를
    // 여기서 놓아 버리면 신호가 끝난 뒤 RefreshStates가 그 자리를 되찾지 못한다.
    public static void CollectSignalExitTasks(IReadOnlyList<UI_ReactionEntry> entries, EUIReactionSignal signal,
        UI_ReactionContext context, CancellationToken token, List<UniTask> results)
    {
        for (int e = 0; e < entries.Count; e++)
        {
            UI_ReactionEntry entry = entries[e];
            if (!entry.MatchesSignal(signal))
            {
                continue;
            }

            List<UI_ReactionBase> reactions = entry.Reactions;
            for (int r = 0; r < reactions.Count; r++)
            {
                if (reactions[r] == null)
                {
                    continue;
                }

                results.Add(reactions[r].ExitAsync(context, entry.ExitPolicy, token));
            }
        }
    }
}
