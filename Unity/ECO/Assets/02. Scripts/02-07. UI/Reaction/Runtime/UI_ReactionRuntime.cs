using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// UI_Reactor가 위임하는 실제 동작. 추적기·디스패처·기준값 저장소를 엮는다.
/// </summary>
public class UI_ReactionRuntime
{
    private readonly UI_ReactionStateTracker _tracker = new UI_ReactionStateTracker();
    private readonly UI_ReactionDispatcher _dispatcher = new UI_ReactionDispatcher();
    private readonly UI_ReactionBaselineStore _baselines = new UI_ReactionBaselineStore();
    private readonly UI_ReactionSignalPlayer _signalPlayer;

    private IReadOnlyList<UI_ReactionEntry> _entries;
    private UI_ReactionContext _context;
    private CancellationToken _token;
    private bool _isLayoutSettled;
    private int _lastActivateFrame = -1;

    public UI_ReactionRuntime()
    {
        // 필드 초기화가 먼저 끝나므로 여기서 디스패처를 넘겨받을 수 있다.
        _signalPlayer = new UI_ReactionSignalPlayer(_dispatcher);
    }

    /// <summary>Disabled 항목이 있을 때만 interactable 폴링이 필요하다.</summary>
    public bool HasDisabledEntry => UI_ReactionEntryScanner.HasState(_entries, EUIReactionState.Disabled);

    public void Init(GameObject owner, IReadOnlyList<UI_ReactionEntry> entries)
    {
        _entries = entries;
        _context = new UI_ReactionContext(owner, _baselines);
        _dispatcher.Init(entries, _tracker, _context);
        _signalPlayer.Init(entries);

        // 매번 새 델리게이트가 잡히지 않도록 한 번만 연결한다.
        _tracker.OnStateChanged = _dispatcher.RefreshStates;
    }

    public void SetToken(CancellationToken token)
    {
        _token = token;
        _dispatcher.SetToken(token);
    }

    public void RefreshStates()
    {
        _dispatcher.RefreshStates();
    }

    public void PlayEvent(EUIReactionEvent uiEvent)
    {
        _dispatcher.PlayEvent(uiEvent);
    }

    // 상태 플래그와 단발 이벤트는 언제나 짝으로 움직인다. 호출부가 둘을 따로 부르며
    // 한쪽을 빠뜨리지 않도록 여기서 묶는다.
    public void SetPointerInside(bool isInside)
    {
        _tracker.SetPointerInside(isInside);
        _dispatcher.PlayEvent(isInside ? EUIReactionEvent.PointerEnter : EUIReactionEvent.PointerExit);
    }

    public void SetPressed(bool isPressed)
    {
        _tracker.SetPressed(isPressed);
        _dispatcher.PlayEvent(isPressed ? EUIReactionEvent.PointerDown : EUIReactionEvent.PointerUp);
    }

    public void SetSelected(bool isSelected)
    {
        _tracker.SetSelected(isSelected);
        _dispatcher.PlayEvent(isSelected ? EUIReactionEvent.Select : EUIReactionEvent.Deselect);
    }

    public void SetInteractable(bool isInteractable)
    {
        _tracker.SetInteractable(isInteractable);
    }

    /// <summary>
    /// 클릭과 Submit 두 경로가 한 프레임에 겹치더라도 실행 연출이 두 번 울리지 않게 막는다.
    /// </summary>
    public void PlayActivate()
    {
        if (_lastActivateFrame == Time.frameCount)
        {
            return;
        }

        _lastActivateFrame = Time.frameCount;
        _dispatcher.PlayEvent(EUIReactionEvent.Activate);
    }

    /// <summary>게임 코드가 요청한 연출. 재생과 취소 처리는 신호 플레이어가 맡는다.</summary>
    public UniTask PlaySignalAsync(EUIReactionSignal signal, CancellationToken cancellationToken)
    {
        return _signalPlayer.PlayAsync(signal, _context, _token, cancellationToken);
    }

    /// <summary>인스펙터 미리보기. 실제 입력 없이 한 상태만 켜 본다.</summary>
    public void PlayStatePreview(EUIReactionState state)
    {
        _tracker.ApplyPreview(state);
        _dispatcher.RefreshStates();
    }

    /// <summary>
    /// uGUI 레이아웃은 PostLateUpdate에 돌기 때문에 첫 프레임의 anchoredPosition은 최종값이 아니다.
    /// 확정 시점에 잠정 기준값을 버려 다음 재생 때 다시 잡히게 하되, 이미 무언가 재생 중이면
    /// 그 값이 기준으로 굳어 버리므로 건너뛴다.
    ///
    /// 자리 장부만 보면 신호 재생을 놓친다. 신호는 자리를 잡지 않아 장부가 비어 있고,
    /// 그 사이 기준값을 지우면 재생이 끝난 뒤 되돌릴 값이 사라진다.
    /// </summary>
    public async UniTaskVoid WaitLayoutSettledAsync(CancellationToken token)
    {
        bool isCanceled = await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token)
            .SuppressCancellationThrow();
        if (isCanceled || _isLayoutSettled)
        {
            return;
        }

        _isLayoutSettled = true;

        if (!_dispatcher.HasOwners && !UI_ReactionEntryScanner.IsAnyPlaying(_entries))
        {
            _baselines.Clear();
        }
    }

    /// <summary>
    /// 비활성화 경로. 풀링된 UI가 같은 프레임에 재사용될 수 있어 전부 동기로 끝낸다.
    /// 순서가 중요하다 — 자리 정리 → 트윈 정지 → 기준값 복원 → 상태 초기화.
    /// 모든 트윈을 먼저 죽여야 복원한 값 위에 남은 트윈이 덮어쓰지 않는다.
    /// </summary>
    public void Deactivate()
    {
        _dispatcher.ClearOwners();
        UI_ReactionEntryScanner.KillAll(_entries);
        UI_ReactionEntryScanner.RestoreAll(_entries, _context);
        _tracker.ResetState();
        _baselines.Clear();
        _isLayoutSettled = false;
    }
}
