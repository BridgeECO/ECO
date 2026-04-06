public static class InputHandler
{
    public static bool IsInputBlocked { get; private set; }

    public static void BlockInput()
    {
        IsInputBlocked = true;
    }

    public static void UnblockInput()
    {
        IsInputBlocked = false;
    }
}
