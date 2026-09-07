using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 컷씬 한 번의 재생을 맡는다. 소유권 획득과 반납, 취소 사유 구분, ESC 감시 구동이 여기 있다.
/// </summary>
public class CutsceneSession
{
    private const float GROUND_WAIT_TIMEOUT = 3f;

    private readonly CutsceneContext _context;
    private readonly CutsceneSkipWatcher _watcher = new CutsceneSkipWatcher();
    private readonly CutsceneRunner _runner;

    private CancellationTokenSource _abortCts;
    private CancellationTokenSource _skipCts;
    private ECutsceneAbortReason _abortReason;

    private GameObject _skipHintObject;
    private bool _isPlaying;
    private bool _isGateEntered;
    private bool _isGateSkippable;

    public bool IsPlaying => _isPlaying;

    public CutsceneSession(CutsceneContext context)
    {
        _context = context;
        _runner = new CutsceneRunner(_watcher);

        _watcher.OnSkipRequested += RequestSkip;
        _watcher.OnHintRequested += () => SetSkipHintVisible(true);

        // 카메라를 누군가 되찾아가려 하면 다투지 않고 접는다. 같은 루트 transform에
        // 두 트윈이 붙으면 매 프레임 마지막 적용값이 이겨 결과가 비결정적이 된다.
        _context.CameraLease.OnContested += HandleCameraContested;
    }

    public async UniTask PlayAsync(CutsceneDirector director, CancellationToken cancellationToken)
    {
        if (_isPlaying || director == null)
        {
            return;
        }

        BeginPlay(director);

        // 외부 토큰과 destroy 토큰을 링크하지 않고 각각 등록만 한다. 링크하면 "누가 취소했는지"를
        // 잃어 사유별 복구 정책이 전부 기본값으로 무너진다.
        using CancellationTokenRegistration externalRegistration =
            cancellationToken.Register(() => Abort(ECutsceneAbortReason.StopInPlace));
        using CancellationTokenRegistration destroyRegistration =
            director.GetCancellationTokenOnDestroy().Register(() => Abort(ECutsceneAbortReason.Destroyed));

        try
        {
            if (director.IsBlockInput)
            {
                _context.InputLease.Acquire();
            }

            if (director.IsFreezePlayer)
            {
                await _context.ActorFreeze.FreezeAsync(_context, true, GROUND_WAIT_TIMEOUT, _abortCts.Token);
            }

            TickWatcherAsync(_abortCts.Token).Forget();

            await _runner.RunAsync(director.Steps, _context, _skipCts.Token, _abortCts.Token);
        }
        catch (OperationCanceledException)
        {
            HandleAbort(director);
        }
        finally
        {
            Finish();
        }
    }

    public void RequestSkip()
    {
        if (!_isPlaying)
        {
            return;
        }

        _skipCts?.Cancel();
    }

    public void Abort(ECutsceneAbortReason reason)
    {
        if (!_isPlaying)
        {
            return;
        }

        _abortReason = reason;
        _abortCts?.Cancel();
    }

    /// <summary>
    /// 빌린 것을 전부 돌려준다. 러너의 finally, 소유자의 OnDisable, 방 리셋, 리스폰 네 경로에서
    /// 각각 독립적으로 불리므로 멱등해야 한다.
    /// </summary>
    public void ForceRelease()
    {
        // 카메라 → 플레이어 → 입력 순. 카메라를 먼저 놓아야 조작이 돌아오는 프레임에
        // 카메라가 이미 플레이어를 따라가고 있다.
        _context.CameraLease.Release();
        _context.ActorFreeze.Release();
        _context.InputLease.Release();
    }

    private void BeginPlay(CutsceneDirector director)
    {
        _isPlaying = true;
        _abortReason = ECutsceneAbortReason.WorldWillReset;
        _abortCts = new CancellationTokenSource();
        _skipCts = CancellationTokenSource.CreateLinkedTokenSource(_abortCts.Token);

        _skipHintObject = director.SkipHintObject;
        _isGateSkippable = director.IsSkippable;
        CutsceneGate.Enter(_isGateSkippable);
        _isGateEntered = true;

        _watcher.Arm(_isGateSkippable);
    }

    private void HandleAbort(CutsceneDirector director)
    {
        if (_abortReason != ECutsceneAbortReason.StopInPlace)
        {
            return;
        }

        // 지형이 반쯤 올라간 채 플레이어가 갇히는 것을 막는다. 스킵과 같은 기계를 쓴다.
        CutsceneRunner.ApplyFinalStatesFrom(director.Steps, _runner.CurrentIndex, _context);
    }

    private void Finish()
    {
        _watcher.Disarm();
        SetSkipHintVisible(false);

        if (_isGateEntered)
        {
            _isGateEntered = false;
            CutsceneGate.Exit(_isGateSkippable);
        }

        ForceRelease();

        _skipCts?.Dispose();
        _skipCts = null;
        _abortCts?.Dispose();
        _abortCts = null;

        _isPlaying = false;
    }

    /// <summary>
    /// 소유자의 Update가 아니라 여기서 돈다. 부모가 비활성화되면 Update는 멈추는데 await 체인은
    /// 계속 돌아, ESC 스킵만 죽고 플레이어는 얼어붙은 채 남는다.
    /// </summary>
    private async UniTaskVoid TickWatcherAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_isPlaying)
            {
                // 일시정지가 걸려도 스킵은 받아야 하므로 unscaled로 잰다.
                _watcher.Tick(Time.unscaledDeltaTime);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void HandleCameraContested()
    {
        Abort(ECutsceneAbortReason.StopInPlace);
    }

    private void SetSkipHintVisible(bool isVisible)
    {
        if (_skipHintObject != null)
        {
            _skipHintObject.SetActive(isVisible);
        }
    }
}
