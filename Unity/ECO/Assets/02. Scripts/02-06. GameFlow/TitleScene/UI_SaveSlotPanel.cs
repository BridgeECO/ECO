using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VInspector;

/// <summary>
/// 이어하기 버튼 클릭 시 표시되는 저장 슬롯 선택 패널.
/// 좌/우 방향키로 슬롯을 순환하며, Escape 키로 패널을 닫는다.
/// </summary>
public class UI_SaveSlotPanel : UI_Popup
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private CanvasGroup _panelCanvasGroup;

    [SerializeField]
    private List<UI_SaveSlotItem> _slotItems;

    private int _selectedIndex = 0;
    private bool _isOpen = false;

    private void Awake()
    {
        InitSlots();
    }

    private void Update()
    {
        if (!_isOpen)
        {
            return;
        }

        HandleKeyboardInput();
    }

    public override void Open()
    {
        base.Open();
        _isOpen = true;
        _selectedIndex = 0;

        RefreshAllSlots();
        ApplySelection();
        PlayOpenAnimation().Forget();
    }

    public override void Close()
    {
        _isOpen = false;
        PlayCloseAnimation(() => base.Close()).Forget();
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _slotItems[_selectedIndex].MoveButtonSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _slotItems[_selectedIndex].MoveButtonSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            _slotItems[_selectedIndex].PressSelectedButton();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.PopupHandler.ClosePopup(this);
        }
    }

    private void ChangeSelection(int offset)
    {
        if (_slotItems == null || _slotItems.Count == 0)
        {
            return;
        }

        _slotItems[_selectedIndex].SetSelected(false);

        _selectedIndex += offset;
        if (_selectedIndex < 0)
        {
            _selectedIndex = _slotItems.Count - 1;
        }
        else if (_selectedIndex >= _slotItems.Count)
        {
            _selectedIndex = 0;
        }

        _slotItems[_selectedIndex].SetSelected(true);
    }

    private void InitSlots()
    {
        if (_slotItems == null)
        {
            return;
        }

        for (int i = 0; i < _slotItems.Count; i++)
        {
            _slotItems[i].Init(i);
        }
    }

    /// <summary>
    /// 패널이 열릴 때마다 각 슬롯의 저장 데이터를 새로 읽어 표시를 갱신한다.
    /// </summary>
    private void RefreshAllSlots()
    {
        for (int i = 0; i < _slotItems.Count; i++)
        {
            _slotItems[i].Init(i);
        }
    }

    private void ApplySelection()
    {
        for (int i = 0; i < _slotItems.Count; i++)
        {
            _slotItems[i].SetSelected(i == _selectedIndex);
        }
    }

    private async UniTaskVoid PlayOpenAnimation()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        _panelCanvasGroup.alpha = 0f;
        await _panelCanvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad).ToUniTask(cancellationToken: ct);
    }

    private async UniTaskVoid PlayCloseAnimation(System.Action onComplete)
    {
        var ct = this.GetCancellationTokenOnDestroy();
        await _panelCanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuad).ToUniTask(cancellationToken: ct);
        onComplete?.Invoke();
    }
}
