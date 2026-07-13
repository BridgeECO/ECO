using System;

public interface IUIConfirm2Buttons
{
    public void Show(Action onConfirm = null, Action onCancel = null);

    public void OnConfirm();

    public void OnCancel();
}

