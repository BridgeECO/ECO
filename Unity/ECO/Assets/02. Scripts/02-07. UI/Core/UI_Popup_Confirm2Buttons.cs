using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UI_Popup_Confirm2Buttons : UI_SystemPopup, IUIConfirm2Buttons
{
    protected abstract Button ConfirmButton { get; }
    protected abstract Button CancelButton { get; }

    protected override List<Button> GetButtons() => new List<Button> { ConfirmButton, CancelButton };

    private Action _onConfirm;
    private Action _onCancel;

    public void Show(string title, string message, Action onConfirm, Action onCancel = null)
    {
        SetPopupText(title, message);
        ClearAllButtonListeners();

        _onConfirm = onConfirm;
        _onCancel = onCancel;

        if (ConfirmButton != null)
        {
            ConfirmButton.onClick.AddListener(OnClickConfirm);
        }
        if (CancelButton != null)
        {
            CancelButton.onClick.AddListener(OnClickCancel);
        }

        UIManager.Instance.PopupHandler.OpenPopup(this);
    }

    private void OnClickConfirm()
    {
        UIManager.Instance.PopupHandler.ClosePopup(this);
        _onConfirm?.Invoke();
    }

    private void OnClickCancel()
    {
        UIManager.Instance.PopupHandler.ClosePopup(this);
        _onCancel?.Invoke();
    }
}
