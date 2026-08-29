using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// 게임 코드가 보낸 신호 하나를 재생하고 묶인 리액션이 모두 끝날 때까지 기다린다.
/// 재생 동안 디스패처를 잠가 상태 연출이 끼어들지 못하게 한다.
/// </summary>
public class UI_ReactionSignalPlayer
{
    private readonly UI_ReactionDispatcher _dispatcher;
    private readonly List<UniTask> _tasks = new List<UniTask>();
    private readonly Action _onCanceled;

    private IReadOnlyList<UI_ReactionEntry> _entries;
    private EUIReactionSignal _playingSignal;

    public UI_ReactionSignalPlayer(UI_ReactionDispatcher dispatcher)
    {
        _dispatcher = dispatcher;

        // 신호를 재생할 때마다 델리게이트가 새로 잡히지 않도록 한 번만 만들어 둔다.
        _onCanceled = KillPlayingSignal;
    }

    public void Init(IReadOnlyList<UI_ReactionEntry> entries)
    {
        _entries = entries;
    }

    /// <summary>호출부의 토큰과 Reactor의 활성 토큰을 함께 묶어, 어느 쪽이 끊겨도 멈춘다.</summary>
    public UniTask PlayAsync(EUIReactionSignal signal, UI_ReactionContext context,
        CancellationToken activeToken, CancellationToken cancellationToken)
    {
        return RunAsync(signal, context, activeToken, cancellationToken, false);
    }

    /// <summary>재생해 둔 신호를 되감는다. 팝업이 닫히기 전에 자식 연출을 먼저 물러나게 할 때 쓴다.</summary>
    public UniTask PlayExitAsync(EUIReactionSignal signal, UI_ReactionContext context,
        CancellationToken activeToken, CancellationToken cancellationToken)
    {
        return RunAsync(signal, context, activeToken, cancellationToken, true);
    }

    // 재생과 되감기는 잠금·취소·대기 처리가 같다. 델리게이트로 가르면 신호마다 클로저가 잡히므로 bool로 가른다.
    private async UniTask RunAsync(EUIReactionSignal signal, UI_ReactionContext context,
        CancellationToken activeToken, CancellationToken cancellationToken, bool isExit)
    {
        if (_entries == null)
        {
            return;
        }

        using CancellationTokenSource linked =
            CancellationTokenSource.CreateLinkedTokenSource(activeToken, cancellationToken);
        if (linked.Token.IsCancellationRequested)
        {
            return;
        }

        // 트윈 완료 신호는 토큰을 보지 않아, 여기서 끊지 않으면 호출부만 풀려나고 연출은 계속 돌아
        // 다음 재생과 값을 다툰다. 신호는 팝업 개폐처럼 드문 경로라 등록 비용을 감수한다.
        _playingSignal = signal;
        using CancellationTokenRegistration registration = linked.Token.Register(_onCanceled);

        _tasks.Clear();
        _dispatcher.Lock();

        try
        {
            if (isExit)
            {
                UI_ReactionEntryScanner.CollectSignalExitTasks(_entries, signal, context, linked.Token, _tasks);
            }
            else
            {
                UI_ReactionEntryScanner.CollectSignalTasks(_entries, signal, context, linked.Token,
                    _dispatcher, _tasks);
            }

            if (0 < _tasks.Count)
            {
                await UniTask.WhenAll(_tasks);
            }
        }
        finally
        {
            _dispatcher.Unlock();
        }

        if (!linked.Token.IsCancellationRequested)
        {
            _dispatcher.RefreshStates();
        }
    }

    // 호출부가 순차로 await하는 경로라 두 신호가 동시에 돌지 않는다. 하나만 들고 있어도 충분하다.
    private void KillPlayingSignal()
    {
        UI_ReactionEntryScanner.KillSignal(_entries, _playingSignal);
    }
}
