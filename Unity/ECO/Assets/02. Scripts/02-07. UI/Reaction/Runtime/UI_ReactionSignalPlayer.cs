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
    public async UniTask PlayAsync(EUIReactionSignal signal, UI_ReactionContext context,
        CancellationToken activeToken, CancellationToken cancellationToken)
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

        // 트윈의 완료 신호는 토큰을 보지 않는다. 취소가 들어왔을 때 여기서 끊어 주지 않으면
        // 기다리던 호출부만 풀려나고 연출은 계속 돌아 다음 재생과 같은 값을 두고 다툰다.
        // 신호는 팝업 개폐나 알림처럼 드문 경로라 등록 비용을 감수한다.
        _playingSignal = signal;
        using CancellationTokenRegistration registration = linked.Token.Register(_onCanceled);

        _tasks.Clear();
        _dispatcher.Lock();

        try
        {
            UI_ReactionEntryScanner.CollectSignalTasks(_entries, signal, context, linked.Token,
                _dispatcher, _tasks);

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

    // 신호는 호출부가 순차로 await하는 경로라 두 신호가 동시에 돌지 않는다.
    // 그래서 재생 중인 신호 하나만 들고 있어도 취소 대상을 특정할 수 있다.
    private void KillPlayingSignal()
    {
        UI_ReactionEntryScanner.KillSignal(_entries, _playingSignal);
    }
}
