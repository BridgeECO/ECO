using System;

public static class InputHandler
{
    public enum EInputMode
    {
        Player,
        UI
    }

    public static bool IsInputBlocked { get; private set; }
    public static EInputMode CurrentMode { get; private set; } = EInputMode.Player;

    public static Action OnCancelEvent;
    public static Action OnNavigateEvent;
    public static Action OnPointEvent;

    public static void BlockInput()
    {
        IsInputBlocked = true;
    }

    public static void UnblockInput()
    {
        IsInputBlocked = false;
    }

    public static void ChangeToUIInput()
    {
        CurrentMode = EInputMode.UI;
        BlockInput(); // 인게임 로직 차단
    }

    public static void ChangeToPlayerInput()
    {
        CurrentMode = EInputMode.Player;
        UnblockInput(); // 인게임 로직 허용
    }

    public static void TriggerCancelEvent() => OnCancelEvent?.Invoke();
    public static void TriggerNavigateEvent() => OnNavigateEvent?.Invoke();
    public static void TriggerPointEvent() => OnPointEvent?.Invoke();
}
