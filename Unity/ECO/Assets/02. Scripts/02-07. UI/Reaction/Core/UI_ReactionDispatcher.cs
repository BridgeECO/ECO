using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 채널 승자를 재생하고 자리를 뺏긴 리액션을 물러나게 한다.
/// 후보 선정은 UI_ReactionStateArbiter가, 자리 장부는 UI_ReactionChannelTable이,
/// 단발 이벤트 재생은 UI_ReactionEventPlayer가 맡는다.
/// </summary>
public class UI_ReactionDispatcher
{
    private readonly UI_ReactionChannelTable _owners = new UI_ReactionChannelTable();
    private readonly UI_ReactionStateArbiter _arbiter = new UI_ReactionStateArbiter();

    private UI_ReactionEventPlayer _eventPlayer;
    private IReadOnlyList<UI_ReactionEntry> _entries;
    private UI_ReactionStateTracker _tracker;
    private UI_ReactionContext _context;
    private CancellationToken _token;
    private int _lockDepth;

    public bool HasOwners => 0 < _owners.Count;

    public void Init(IReadOnlyList<UI_ReactionEntry> entries, UI_ReactionStateTracker tracker,
        UI_ReactionContext context)
    {
        _entries = entries;
        _tracker = tracker;
        _context = context;
        _eventPlayer = new UI_ReactionEventPlayer(_owners, RefreshStates);
    }

    public void SetToken(CancellationToken token)
    {
        _token = token;
    }

    /// <summary>Signal 재생 동안 상태 리액션이 끼어들지 못하게 막는다.</summary>
    public void Lock()
    {
        _lockDepth++;
    }

    public void Unlock()
    {
        if (0 < _lockDepth)
        {
            _lockDepth--;
        }
    }

    public void RefreshStates()
    {
        if (_entries == null || 0 < _lockDepth)
        {
            return;
        }

        _arbiter.Collect(_entries, _tracker, _context);
        ReleaseLosers();
        StartWinners();
    }

    public void PlayEvent(EUIReactionEvent uiEvent)
    {
        if (_entries == null)
        {
            return;
        }

        _eventPlayer.Play(_entries, uiEvent, _context, _token);
    }

    /// <summary>자리 장부만 비운다. 리액션 정지는 호출부가 따로 처리한다.</summary>
    public void ClearOwners()
    {
        _owners.Clear();
        _arbiter.Clear();
    }

    /// <summary>
    /// 이 리액션이 쓰려는 자리를 쥐고 있는 다른 리액션을 끊고 자리를 비운다.
    /// Signal이 상태 연출을 밀어내고 값을 가져갈 때 쓴다.
    /// </summary>
    public void KillChannel(UI_ReactionBase reaction, UI_ReactionContext context)
    {
        GameObject target = reaction.ResolveTarget(context);
        if (target == null)
        {
            return;
        }

        int targetId = target.GetInstanceID();
        if (!_owners.TryGet(targetId, reaction.Channel, out UI_ReactionChannelOwner owner))
        {
            return;
        }

        if (owner.Reaction != reaction)
        {
            owner.Reaction.Kill();
        }

        _owners.Release(targetId, reaction.Channel, owner.Reaction);
    }

    private void ReleaseLosers()
    {
        for (int i = _owners.Count - 1; 0 <= i; i--)
        {
            UI_ReactionChannelOwner owner = _owners[i];

            // 단발 이벤트가 잡고 있는 자리는 그 재생이 끝날 때 스스로 놓는다.
            if (owner.Priority == UI_ReactionEventPlayer.EVENT_PRIORITY)
            {
                continue;
            }

            bool hasSuccessor = _arbiter.TryGet(owner.TargetId, owner.Channel,
                out UI_ReactionChannelOwner candidate);
            if (hasSuccessor && candidate.Reaction == owner.Reaction)
            {
                continue;
            }

            _owners.RemoveAt(i);

            // 자리를 이어받을 리액션이 있으면 물러나는 연출은 재생하지 않고 끊기만 한다.
            // 두 트윈이 같은 값을 동시에 쓰면 프레임마다 나중에 갱신된 쪽이 이겨 떨리고,
            // 물러나는 쪽이 더 길면 새 연출이 끝난 뒤 값을 기준값으로 끌고 가 버린다.
            // 누름을 뗄 때 Pressed가 물러나고 Hover가 들어오는 경로가 정확히 이 경우다.
            if (hasSuccessor)
            {
                owner.Reaction.Kill();
                continue;
            }

            owner.Reaction.ExitAsync(_context, owner.ExitPolicy, _token).Forget();
        }
    }

    private void StartWinners()
    {
        for (int i = 0; i < _arbiter.Count; i++)
        {
            UI_ReactionChannelOwner candidate = _arbiter[i];

            if (_owners.TryGet(candidate.TargetId, candidate.Channel, out UI_ReactionChannelOwner current)
                && (current.Reaction == candidate.Reaction
                    || current.Priority == UI_ReactionEventPlayer.EVENT_PRIORITY))
            {
                continue;
            }

            _owners.Set(candidate);
            candidate.Reaction.PlayAsync(_context, candidate.InterruptPolicy, _token).Forget();
        }
    }
}
