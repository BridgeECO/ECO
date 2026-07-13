using System;

public interface IUIConfirm2Buttons
{
    public void Show(Action onConfirm = null, Action onCancel = null);
}

