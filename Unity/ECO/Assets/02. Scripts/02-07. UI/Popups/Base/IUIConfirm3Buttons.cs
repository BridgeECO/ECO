using System;

public interface IUIConfirm3Buttons
{
    public void Show(string title, string message, Action onAction1, Action onAction2, Action onCancel = null);
}
