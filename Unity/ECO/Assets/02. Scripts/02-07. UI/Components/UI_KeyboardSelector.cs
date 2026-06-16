using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class UI_KeyboardSelector : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<UI_ButtonSelectionItem> _items;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    private int _currentIndex;
    private UI_Popup _parentPopup;

    #region Unity Lifecycle Methods
    private void Awake()
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponentInParent<CanvasGroup>();
        }
        _parentPopup = GetComponentInParent<UI_Popup>();
    }

    private void Start()
    {
        InitSelection();
    }

    private void Update()
    {
        HandleKeyboardInput();
    }
    #endregion

    #region Input Processings
    private void HandleKeyboardInput()
    {
        if (_canvasGroup != null && !_canvasGroup.interactable)
        {
            return;
        }

        if (_parentPopup != null && _parentPopup.IsClosing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangeSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangeSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            SelectCurrentButton();
        }
    }
    #endregion

    #region Logic & Visuals
    private void InitSelection()
    {
        _currentIndex = 0;
        RefreshSelection();
    }

    private void ChangeSelection(int offset)
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
        else if (_currentIndex >= _items.Count)
        {
            _currentIndex = 0;
        }

        RefreshSelection();
    }

    private void RefreshSelection()
    {
        if (_items == null || _items.Count == 0)
        {
            return;
        }

        for (int i = 0; i < _items.Count; i++)
        {
            bool isSelected = (i == _currentIndex);
            
            if (_items[i].SelectionArrows != null)
            {
                foreach (GameObject arrow in _items[i].SelectionArrows)
                {
                    if (arrow != null)
                    {
                        arrow.SetActive(isSelected);
                    }
                }
            }
        }
    }

    private void SelectCurrentButton()
    {
        if (_items == null || _items.Count == 0 || _currentIndex < 0 || _currentIndex >= _items.Count)
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
}
