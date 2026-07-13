using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public abstract class UI_Popup_Confirm2Buttons : UI_SystemPopup, IUIConfirm2Buttons
{
    protected abstract Button ConfirmButton { get; }
    protected abstract Button CancelButton { get; }

    protected override List<Button> GetButtons() => new List<Button> { ConfirmButton, CancelButton };

    private Action _onConfirm;
    private Action _onCancel;

    public void Show(Action onConfirm = null, Action onCancel = null)
    {
        ClearAllButtonListeners();

        _onConfirm = onConfirm;
        _onCancel = onCancel;

        ConfirmButton?.onClick.AddListener(OnClickConfirm);
        CancelButton?.onClick.AddListener(OnClickCancel);
        UIManager.Instance.PopupHandler.OpenPopup(this);
    }

    public virtual void OnConfirm()
    {
        _onConfirm?.Invoke();
    }

    public virtual void OnCancel()
    {
        _onCancel?.Invoke();
    }

    private async void OnClickConfirm()
    {
        var token = this.GetCancellationTokenOnDestroy();
        await UIManager.Instance.PopupHandler.ClosePopupAsync(this);
        if (token.IsCancellationRequested)
        {
            return;
        }
        OnConfirm();
    }

    private async void OnClickCancel()
    {
        var token = this.GetCancellationTokenOnDestroy();
        await UIManager.Instance.PopupHandler.ClosePopupAsync(this);
        if (token.IsCancellationRequested)
        {
            return;
        }
        OnCancel();
    }
}
