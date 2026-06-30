using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VInspector;

/// <summary>
/// 이어하기/새 게임 버튼 클릭 시 표시되는 저장 슬롯 선택 패널.
/// 좌/우 방향키로 슬롯을 순환하며, 상/하 방향키로 슬롯 내 버튼을 이동한다.
/// Escape 키는 UIManager 전역 처리에 위임하며, OpenAsync/CloseAsync 시점에
/// UI_KeyboardInputManager에 자동 등록/해제된다.
/// </summary>
public class UI_SaveSlotPopup : UI_Popup, IKeyboardControllable
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private CanvasGroup _panelCanvasGroup;

    [SerializeField]
    private List<UI_SaveSlotItem> _slotItems;

    private int _selectedIndex = 0;

    protected override void Awake()
    {
        base.Awake();
        InitSlots();
    }

    public async UniTask OpenAsync(ESlotPanelMode mode)
    {
        await base.OpenAsync();
        _selectedIndex = 0;

        RefreshAllSlots(mode);
        ApplySelection();
        UI_KeyboardInputManager.Instance.PushHandler(this);
        await PlayOpenAnimation();
    }

    public override async UniTask OpenAsync()
    {
        await OpenAsync(ESlotPanelMode.Continue);
    }

    public override async UniTask CloseAsync()
    {
        // 애니메이션 시작 전에 Pop하여 닫히는 도중 입력이 처리되지 않도록 함
        UI_KeyboardInputManager.Instance.PopHandler();
        await PlayCloseAnimation();
        await base.CloseAsync();
    }

    #region IKeyboardControllable
    public void OnMoveLeft()
    {
        ChangeSelection(-1);
    }

    public void OnMoveRight()
    {
        ChangeSelection(1);
    }

    public void OnMoveUp()
    {
        _slotItems[_selectedIndex].MoveButtonSelection(-1);
    }

    public void OnMoveDown()
    {
        _slotItems[_selectedIndex].MoveButtonSelection(1);
    }

    public void OnConfirm()
    {
        _slotItems[_selectedIndex].PressSelectedButton();
    }

    // ESC는 UIManager가 CloseLatestPopup으로 전역 처리하므로 여기서는 응답하지 않음
    public void OnCancel() { }
    #endregion

    #region Slot Management
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
        else if (_slotItems.Count<= _selectedIndex )
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
            _slotItems[i].Init(i, ESlotPanelMode.Continue);
        }
    }

    /// <summary>
    /// 패널이 열릴 때마다 각 슬롯의 저장 데이터와 모드를 새로 읽어 표시를 갱신한다.
    /// </summary>
    private void RefreshAllSlots(ESlotPanelMode mode)
    {
        for (int i = 0; i < _slotItems.Count; i++)
        {
            _slotItems[i].Init(i, mode);
        }
    }

    private void ApplySelection()
    {
        for (int i = 0; i < _slotItems.Count; i++)
        {
            _slotItems[i].SetSelected(i == _selectedIndex);
        }
    }
    #endregion

    #region Animations
    private async UniTask PlayOpenAnimation()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        _panelCanvasGroup.alpha = 0f;
        await _panelCanvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad).ToUniTask(cancellationToken: ct);
    }

    private async UniTask PlayCloseAnimation()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        await _panelCanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuad).ToUniTask(cancellationToken: ct);
    }
    #endregion
}
