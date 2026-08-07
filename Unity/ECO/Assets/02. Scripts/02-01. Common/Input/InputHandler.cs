using System;
using UnityEngine;

public static class InputHandler
{
    public static Action OnNavigateEvent;
    public static Action OnPointEvent;

    // 리스폰·씬 전환·시네마틱 등 여러 시스템이 동시에 입력을 차단할 수 있으므로
    // bool 토글이 아닌 참조 카운팅으로 관리한다. (한쪽의 해제가 다른 쪽의 차단을 풀지 않도록)
    private static int _blockCount;
    // UI 모드는 팝업이 중첩될 때마다 반복 호출되므로 카운트가 아닌 멱등 플래그로 관리한다.
    private static bool _isUIMode;

    public static bool IsInputBlocked => _isUIMode || 0 < _blockCount;

    public static void BlockInput()
    {
        _blockCount++;
    }

    public static void UnblockInput()
    {
        _blockCount = Mathf.Max(0, _blockCount - 1);
    }

    public static void ChangeToUIInput()
    {
        _isUIMode = true;
    }

    public static void ChangeToPlayerInput()
    {
        _isUIMode = false;
    }

    public static void TriggerNavigateEvent() => OnNavigateEvent?.Invoke();
    public static void TriggerPointEvent() => OnPointEvent?.Invoke();

    // 도메인 리로드 비활성화(Enter Play Mode Options) 환경에서
    // 이전 플레이 세션의 static 상태가 잔류하지 않도록 초기화한다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitStaticState()
    {
        OnNavigateEvent = null;
        OnPointEvent = null;
        _blockCount = 0;
        _isUIMode = false;
    }
}
