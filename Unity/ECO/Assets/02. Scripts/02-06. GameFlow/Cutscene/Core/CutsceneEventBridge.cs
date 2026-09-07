using System;

/// <summary>
/// 컷씬이 관심 있는 전역 이벤트를 한 곳에서 구독한다.
///
/// 씬 전환은 구독하지 않는다. 리전 씬이 언로드되면 Director가 파괴되어 destroy 토큰이
/// 먼저 발화하고, 그쪽이 "아무것도 쓰지 않는다"라는 더 안전한 정책을 탄다.
/// 곧 사라질 오브젝트에 최종 상태를 쓰는 것은 이득이 없다.
/// </summary>
public class CutsceneEventBridge
{
    public Action<ECutsceneAbortReason> OnAbortRequested;
    public Action OnSaveSnapshotRequested;

    private bool _isListenerAdded;

    /// <summary>
    /// EventManager.Instance는 로드 순서에 따라 null일 수 있다.
    /// 소유자가 OnEnable과 Start에서 두 번 부르는 것을 전제로 멱등하게 만든다.
    /// </summary>
    public void TryAddListeners()
    {
        if (_isListenerAdded || EventManager.Instance == null)
        {
            return;
        }

        EventManager.Instance.AddEventListener(EEventType.PlayerDied, HandleWorldWillReset);
        EventManager.Instance.AddEventListener(EEventType.RespawnReset, HandleWorldWillReset);
        EventManager.Instance.AddEventListener(EEventType.SavePointUpdated, HandleSavePointUpdated);
        _isListenerAdded = true;
    }

    public void RemoveListeners()
    {
        if (!_isListenerAdded)
        {
            return;
        }

        _isListenerAdded = false;
        if (!EventManager.HasInstance)
        {
            return;
        }

        EventManager.Instance.RemoveEventListener(EEventType.PlayerDied, HandleWorldWillReset);
        EventManager.Instance.RemoveEventListener(EEventType.RespawnReset, HandleWorldWillReset);
        EventManager.Instance.RemoveEventListener(EEventType.SavePointUpdated, HandleSavePointUpdated);
    }

    private void HandleWorldWillReset()
    {
        OnAbortRequested?.Invoke(ECutsceneAbortReason.WorldWillReset);
    }

    private void HandleSavePointUpdated()
    {
        OnSaveSnapshotRequested?.Invoke();
    }
}
