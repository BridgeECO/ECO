using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// 항목 목록을 통째로 훑는 일만 모아 둔다. 상태를 갖지 않아 전부 정적 메서드다.
/// </summary>
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
}
