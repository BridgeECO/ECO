using System;

public interface IUIConfirm2Buttons
{
    public void Show(string title, string message, Action onConfirm, Action onCancel = null);
}
