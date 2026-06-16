using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UI_PopupHandler
{
    private Stack<UI_Popup> _popups = new Stack<UI_Popup>();
    private bool _isClosing = false;

    public bool HasPopups => _popups.Count > 0;

    public void Init()
    {
        // 이벤트 구독을 UIManager로 중앙집중화하여 중복 호출 방지
    }

    public void Dispose()
    {
    }

    public void OpenPopup(UI_Popup popup)
    {
        if (popup.gameObject.activeSelf)
        {
            return;
        }

        SetTopPopupFocusState(false);

        _popups.Push(popup);
        popup.OpenAsync().Forget();

        InputHandler.ChangeToUIInput();
        SetPopupFocusState(popup, true);
    }

    public void OpenSaveSlotPopup(UI_SaveSlotPopup panel, ESlotPanelMode mode)
    {
        if (panel.gameObject.activeSelf)
        {
            return;
        }

        SetTopPopupFocusState(false);

        _popups.Push(panel);
        panel.OpenAsync(mode).Forget();

        InputHandler.ChangeToUIInput();
        SetPopupFocusState(panel, true);
    }

    public void ClosePopup(UI_Popup popup)
    {
        if (_popups.TryPeek(out UI_Popup latestPopup) && latestPopup == popup)
        {
            CloseLatestPopup();
        }
    }

    public void CloseLatestPopup()
    {
        if (_isClosing) return;
        CloseLatestPopupAsync().Forget();
    }

    private async UniTaskVoid CloseLatestPopupAsync()
    {
        if (_popups.TryPop(out UI_Popup latestPopup))
        {
            _isClosing = true;
            SetPopupFocusState(latestPopup, false);

            try
            {
                await latestPopup.CloseAsync();
            }
            finally
            {
                _isClosing = false;
            }

            if (_popups.Count <= 0)
            {
                InputHandler.ChangeToPlayerInput();
                return;
            }

            SetTopPopupFocusState(true);
        }
    }

    private void SetTopPopupFocusState(bool state)
    {
        if (_popups.TryPeek(out UI_Popup topPopup))
        {
            SetPopupFocusState(topPopup, state);
        }
    }

    private void SetPopupFocusState(UI_Popup popup, bool state)
    {
        if (popup != null && popup.TryGetComponent<UI_FocusController>(out var selector))
        {
            selector.enabled = state;
        }
    }
}


