using System;

public interface IUIConfirm3Buttons
{
    public void Show(Action onAction1 = null, Action onAction2 = null, Action onCancel = null);
}

