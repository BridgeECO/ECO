using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 저장 슬롯의 동작 버튼(이어하기/새 게임)과 지우기 버튼의 활성 상태와 키보드 포커스를 다룬다.
/// 키보드 입력은 UI_KeyboardInputManager가 세이브 슬롯 팝업을 거쳐 위임하므로, 위아래 이동은
/// EventSystem 자동 내비게이션이 아니라 이 클래스가 직접 처리하고 포커스는 EventSystem 선택으로 표시한다.
/// </summary>
public class UI_SaveSlotButtons
{
    private readonly Button _actionButton;
    private readonly TextMeshProUGUI _actionButtonText;
    private readonly Button _deleteButton;

    public bool IsActionSelected => IsSelected(_actionButton);
    public bool IsDeleteSelected => IsSelected(_deleteButton);

    public UI_SaveSlotButtons(Button actionButton, TextMeshProUGUI actionButtonText, Button deleteButton)
    {
        _actionButton = actionButton;
        _actionButtonText = actionButtonText;
        _deleteButton = deleteButton;

        Navigation noneNav = new Navigation { mode = Navigation.Mode.None };
        _actionButton.navigation = noneNav;
        _deleteButton.navigation = noneNav;
    }

    public void RefreshLabel(ESlotPanelMode mode)
    {
        _actionButtonText.text = mode == ESlotPanelMode.NewGame ? "새 게임" : "이어하기";
    }

    // 누를 수 있는 동작 버튼에 포커스를 둔다. 동작 버튼을 못 누르면 포커스를 비워 엔터가 어떤 버튼에도 닿지 않게 한다.
    public void Refresh(bool canAct, bool canDelete)
    {
        _actionButton.interactable = canAct;
        _deleteButton.interactable = canDelete;
        SetSelectedGameObject(canAct ? _actionButton.gameObject : null);
    }

    public void MoveSelection(int direction)
    {
        if (IsActionSelected && direction == 1 && _deleteButton.interactable)
        {
            SetSelectedGameObject(_deleteButton.gameObject);
        }
        else if (IsDeleteSelected && direction == -1 && _actionButton.interactable)
        {
            SetSelectedGameObject(_actionButton.gameObject);
        }
    }

    private static bool IsSelected(Button button)
    {
        return EventSystem.current != null && EventSystem.current.currentSelectedGameObject == button.gameObject;
    }

    private static void SetSelectedGameObject(GameObject target)
    {
        if (EventSystem.current == null)
        {
            return;
        }
        EventSystem.current.SetSelectedGameObject(target);
    }
}
