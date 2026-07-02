using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VInspector;

/// <summary>
/// 타이틀씬 수직 버튼 리스트의 키보드 조작 핸들러.
/// 상/하 방향키로 버튼을 순환하며, Enter로 현재 버튼을 클릭한다.
/// OnEnable/OnDisable 시점에 UI_KeyboardInputManager에 자동 등록/해제된다.
/// </summary>
public class UI_TitleButtonsKeyboardHandler : MonoBehaviour, IKeyboardControllable
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<UI_ButtonSelectionItem> _items;

    private TitleScene _titleScene;
    private int _currentIndex;
    private bool _isMenuStarted;

    private List<UnityAction> _clickActions;

    #region Unity Lifecycle Methods
    private void OnEnable()
    {
        InitSelection();

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

        if (_isMenuStarted && UI_KeyboardInputManager.HasInstance)
        {
            UI_KeyboardInputManager.Instance.PushHandler(this);
        }
    }

    private void OnDisable()
    {
        SafePopHandler();

        if (_clickActions != null)
        {
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
    }

    private void OnDestroy()
    {
        if (_titleScene != null)
        {
            _titleScene.OnMenuStarted -= HandleMenuStarted;
        }
    }
    #endregion

    public void Init(TitleScene titleScene)
    {
        _titleScene = titleScene;
        if (_titleScene != null)
        {
            _titleScene.OnMenuStarted += HandleMenuStarted;
        }
    }

    private void HandleMenuStarted()
    {
        _isMenuStarted = true;
        if (UI_KeyboardInputManager.HasInstance)
        {
            UI_KeyboardInputManager.Instance.PushHandler(this);
        }
    }

    #region IKeyboardControllable
    public void OnMoveUp()
    {
        ChangeSelection(-1);
    }

    public void OnMoveDown()
    {
        ChangeSelection(1);
    }

    // 타이틀씬은 수직 리스트만 지원하므로 좌/우 입력은 비활성화
    public void OnMoveLeft() { }

    public void OnMoveRight() { }

    public void OnConfirm()
    {
        SelectCurrentButton();
    }

    // ESC는 UIManager가 전역 처리하므로 여기서는 응답하지 않음
    public void OnCancel() { }
    #endregion

    #region Selection Logic
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
        else if (_items.Count<= _currentIndex )
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

            if (_items[i].SelectionArrows == null)
            {
                continue;
            }

            for (int j = 0; j < _items[i].SelectionArrows.Count; j++)
            {
                GameObject arrow = _items[i].SelectionArrows[j];

                if (arrow != null)
                {
                    arrow.SetActive(isSelected);
                }
            }
        }
    }

    private void SelectCurrentButton()
    {
        if (_items == null || _items.Count == 0)
        {
            return;
        }

        if (_currentIndex < 0 || _items.Count<= _currentIndex )
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

    #region Cleanup
    // OnDisable 내부에서 직접 싱글톤 접근을 지양하는 컨벤션 준수를 위해 별도 메서드로 분리
    private void SafePopHandler()
    {
        if (UI_KeyboardInputManager.HasInstance)
        {
            UI_KeyboardInputManager.Instance.PopHandler(this);
        }
    }
    #endregion
}
