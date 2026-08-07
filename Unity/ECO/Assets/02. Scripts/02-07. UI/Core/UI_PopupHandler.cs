using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UI_PopupHandler
{
    private Stack<UI_Popup> _popups = new Stack<UI_Popup>();
    private bool _isClosing = false;

    public bool HasPopups => 0< _popups.Count ;

    public void Init()
    {
        // 이벤트 구독을 UIManager로 중앙집중화하여 중복 호출 방지
    }

    public void Dispose()
    {
    }

    public void ClearAllPopups()
    {
        while (_popups.Count > 0)
        {
            var popup = _popups.Pop();
            if (popup != null)
            {
                popup.gameObject.SetActive(false);
            }
        }
        _isClosing = false;
        InputHandler.ChangeToPlayerInput();
    }

    public void OpenPopup(UI_Popup popup)
    {
        if (popup.gameObject.activeSelf)
        {
            return;
        }

        // 늦게 등장하는 팝업(씬 로드 후 열리는 팝업 포함)도 닫기 경로를 갖도록 여기서 주입한다.
        popup.SetHandler(this);
        SetTopPopupFocusState(false);

        _popups.Push(popup);
        popup.OpenAsync().Forget();

        InputHandler.ChangeToUIInput();
        SetPopupFocusState(popup, true);
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

    public async UniTask CloseLatestPopupAsync()
    {
        if (_isClosing) return;

        if (_popups.TryPop(out UI_Popup latestPopup))
        {
            _isClosing = true;
            SetPopupFocusState(latestPopup, false);

            try
            {
                if (latestPopup != null)
                {
                    await latestPopup.CloseAsync();
                }
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

    public async UniTask ClosePopupAsync(UI_Popup popup)
    {
        if (_popups.TryPeek(out UI_Popup latestPopup) && latestPopup == popup)
        {
            await CloseLatestPopupAsync();
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


