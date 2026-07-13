using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UI_Popup_Confirm3Buttons : UI_SystemPopup, IUIConfirm3Buttons
{
    protected abstract Button Action1Button { get; }
    protected abstract Button Action2Button { get; }
    protected abstract Button CancelButton { get; }

    protected override List<Button> GetButtons() => new List<Button> { Action1Button, Action2Button, CancelButton };

    private Action _onAction1;
    private Action _onAction2;
    private Action _onCancel;

    public void Show(string title, string message, Action onAction1, Action onAction2, Action onCancel = null)
    {
        SetPopupText(title, message);
        ClearAllButtonListeners();

        _onAction1 = onAction1;
        _onAction2 = onAction2;
        _onCancel = onCancel;

        if (Action1Button != null)
        {
            Action1Button.onClick.AddListener(OnClickAction1);
        }
        if (Action2Button != null)
        {
            Action2Button.onClick.AddListener(OnClickAction2);
        }
        if (CancelButton != null)
        {
            CancelButton.onClick.AddListener(OnClickCancel);
        }

        UIManager.Instance.PopupHandler.OpenPopup(this);
    }

    private void OnClickAction1()
    {
        UIManager.Instance.PopupHandler.ClosePopup(this);
        _onAction1?.Invoke();
    }

    private void OnClickAction2()
    {
        UIManager.Instance.PopupHandler.ClosePopup(this);
        _onAction2?.Invoke();
    }

    private void OnClickCancel()
    {
        UIManager.Instance.PopupHandler.ClosePopup(this);
        _onCancel?.Invoke();
    }
}
