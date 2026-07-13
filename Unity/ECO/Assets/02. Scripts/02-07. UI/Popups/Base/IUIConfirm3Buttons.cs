using System;

public interface IUIConfirm3Buttons
{
    public void Show(Action onAction1 = null, Action onAction2 = null, Action onCancel = null);

    public void OnAction1();

    public void OnAction2();

    public void OnCancel();
}

