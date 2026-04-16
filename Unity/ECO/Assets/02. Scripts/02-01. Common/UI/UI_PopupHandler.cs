using System.Collections.Generic;
using UnityEngine;

public class UI_PopupHandler : MonoBehaviourSingleton<UI_PopupHandler>
{
    private Stack<UI_Popup> _popups = new Stack<UI_Popup>();

    public bool HasPopups => _popups.Count > 0;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // ESC key to close current popup
        InputHandler.OnCancelEvent += OnCancelPressed;
    }

    private void OnDisable()
    {
        InputHandler.OnCancelEvent -= OnCancelPressed;
    }

    private void Update()
    {
        // 별도의 Input 시스템에서 OnCancelEvent 이벤트를 발생시키지 않는다면, 여기서 직접 수신
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            InputHandler.TriggerCancelEvent();
        }
    }

    public void OpenPopup(UI_Popup popup)
    {
        if (popup.gameObject.activeSelf)
        {
            return;
        }

        SetTopPopupFocusState(false);

        _popups.Push(popup);
        popup.Open();

        InputHandler.ChangeToUIInput();
        SetPopupFocusState(popup, true);
    }

    public void ClosePopup(UI_Popup popup)
    {
        if (_popups.TryPeek(out UI_Popup latestPopup) && ReferenceEquals(latestPopup, popup))
        {
            CloseLatestPopup();
        }
    }

    private void CloseLatestPopup()
    {
        if (_popups.TryPop(out UI_Popup latestPopup))
        {
            latestPopup.Close();

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

    private void OnCancelPressed()
    {
        if (_popups.Count > 0)
        {
            CloseLatestPopup();
        }
    }
}
