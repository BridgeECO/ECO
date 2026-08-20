using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 타이틀 메뉴 버튼 목록의 선택 상태를 관리한다.
/// 인덱스 순환, EventSystem 반영, 현재 버튼 실행, 마우스 클릭과의 인덱스 동기화를 담당한다.
/// 유니티 생명주기와 키보드 매니저 등록은 UI_TitleButtonsKeyboardHandler가 맡는다.
/// </summary>
public class TitleButtonSelector
{
    private readonly List<UI_ButtonSelectionItem> _items;

    private List<UnityAction> _clickActions;
    private int _currentIndex;

    /// <summary>메뉴 인트로가 끝나기 전에는 선택 표시를 억제한다.</summary>
    public bool IsSelectionEnabled { get; private set; }

    public TitleButtonSelector(List<UI_ButtonSelectionItem> items)
    {
        _items = items;
    }

    #region Selection Logic
    public void SetSelectionEnabled(bool isEnabled)
    {
        IsSelectionEnabled = isEnabled;
    }

    public void InitSelection()
    {
        _currentIndex = 0;
        RefreshSelection();
    }

    public void ChangeSelection(int offset)
    {
        if (_items == null || _items.Count == 0)
        {
            return;
        }

        _currentIndex += offset;

        if (_currentIndex < 0)
        {
            _currentIndex = _items.Count - 1;
        }
        else if (_items.Count <= _currentIndex)
        {
            _currentIndex = 0;
        }

        RefreshSelection();
    }

    /// <summary>
    /// 무엇이 선택됐는지만 정한다. 선택 표시는 각 버튼의 UI_Reactor가 Selected 항목으로 맡는다.
    /// </summary>
    public void RefreshSelection()
    {
        // 메뉴가 시작되기 전에 선택을 강제하면 타이틀 인트로 도중에 버튼이 눌린 것처럼 보인다.
        if (!IsSelectionEnabled || _items == null || _items.Count == 0)
        {
            return;
        }

        if (_currentIndex < 0 || _items.Count <= _currentIndex)
        {
            return;
        }

        Button target = _items[_currentIndex].TargetButton;
        if (target == null || EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(target.gameObject);
    }

    public void InvokeCurrentButton()
    {
        if (_items == null || _items.Count == 0)
        {
            return;
        }

        if (_currentIndex < 0 || _items.Count <= _currentIndex)
        {
            return;
        }

        UI_ButtonSelectionItem currentItem = _items[_currentIndex];

        if (currentItem.TargetButton != null)
        {
            currentItem.TargetButton.onClick.Invoke();
        }
    }
    #endregion

    #region Click Synchronization
    // 마우스로 버튼을 누르면 키보드 인덱스도 그 버튼으로 옮겨야 다음 방향키가 자연스럽게 이어진다.
    public void BindClickListeners()
    {
        if (_items == null)
        {
            return;
        }

        _clickActions = new List<UnityAction>();
        for (int i = 0; i < _items.Count; i++)
        {
            int index = i;
            UnityAction action = () =>
            {
                _currentIndex = index;
                RefreshSelection();
            };
            _clickActions.Add(action);

            if (_items[i].TargetButton != null)
            {
                _items[i].TargetButton.onClick.AddListener(action);
            }
        }
    }

    public void UnbindClickListeners()
    {
        if (_clickActions == null || _items == null)
        {
            return;
        }

        for (int i = 0; i < _items.Count; i++)
        {
            if (i < _clickActions.Count && _items[i].TargetButton != null)
            {
                _items[i].TargetButton.onClick.RemoveListener(_clickActions[i]);
            }
        }

        _clickActions.Clear();
        _clickActions = null;
    }
    #endregion
}
