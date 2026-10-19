using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 저장 슬롯 하나의 시각적 표현과 버튼 인터랙션을 담당한다.
/// 선택 상태 변경 시 이어하기/선택 버튼과 지우기 버튼이 나타난다.
/// Init(slotIndex, mode)로 패널 모드를 주입받아 버튼 동작을 분기한다.
/// 버튼 쌍의 활성 상태와 키보드 포커스 이동은 UI_SaveSlotButtons가 맡는다.
/// </summary>
public class UI_SaveSlotItem : MonoBehaviour, IPointerClickHandler
{
    public Action<int> OnClicked;

    [Foldout("Hierarchy")]
    [SerializeField]
    private VerticalLayoutGroup _verticalLayoutGroup;

    [SerializeField]
    private GameObject _buttonContinueAndDelete;

    [SerializeField]
    private TextMeshProUGUI _slotNumberText;

    [SerializeField]
    private TextMeshProUGUI _regionNameText;

    [SerializeField]
    private Button _continueButton;

    [SerializeField]
    private TextMeshProUGUI _continueButtonText;

    [SerializeField]
    private Button _deleteButton;

    // 새 게임 시작 시 진입할 씬. 프리팹에서 설정한다. (코드 하드코딩 제거)
    [Foldout("Settings")]
    [SerializeField]
    private ESceneNames _newGameScene = ESceneNames.Scrap_Nest;

    private int _slotIndex;
    private bool _hasSaveData;
    private ESlotPanelMode _mode;
    private SaveSlotPresenter _presenter;
    private UI_SaveSlotButtons _buttons;

    private void Awake()
    {
        _continueButton.onClick.AddListener(OnClickActionBtn);
        _deleteButton.onClick.AddListener(OnClickDeleteBtn);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke(_slotIndex);
    }

    private void OnDestroy()
    {
        _continueButton.onClick.RemoveListener(OnClickActionBtn);
        _deleteButton.onClick.RemoveListener(OnClickDeleteBtn);
    }

    public void Init(int slotIndex, ESlotPanelMode mode)
    {
        _slotIndex = slotIndex;
        _mode = mode;
        _presenter = new SaveSlotPresenter(slotIndex, _newGameScene);
        _slotNumberText.text = (slotIndex + 1).ToString();

        // 팝업의 Awake가 이 메서드를 부를 때는 슬롯의 Awake가 아직 돌지 않았을 수 있어 여기서 만든다.
        if (_buttons is null)
        {
            _buttons = new UI_SaveSlotButtons(_continueButton, _continueButtonText, _deleteButton);
        }

        _buttons.RefreshLabel(mode);
        LoadAndRefreshSlot();
        ResetToDeselectedState();
    }

    public void SetSelected(bool isSelected)
    {
        if (!isSelected)
        {
            ResetToDeselectedState();
            return;
        }

        _buttonContinueAndDelete.SetActive(true);
        _verticalLayoutGroup.spacing = 30f;

        // Continue 모드: 저장 데이터가 있을 때만 이어하기 활성화
        // NewGame 모드: 항상 슬롯 선택 가능 (빈 슬롯도 선택 가능)
        _buttons.Refresh(_mode == ESlotPanelMode.NewGame || _hasSaveData, _hasSaveData);
    }

    // 외부에서 상하 방향키 입력 시 호출
    public void MoveButtonSelection(int direction)
    {
        _buttons.MoveSelection(direction);
    }

    // 외부에서 엔터/스페이스바 입력 시 호출
    public void PressSelectedButton()
    {
        if (_buttons.IsActionSelected)
        {
            OnClickActionBtn();
        }
        else if (_buttons.IsDeleteSelected)
        {
            OnClickDeleteBtn();
        }
    }

    private void ResetToDeselectedState()
    {
        _buttonContinueAndDelete.SetActive(false);
        _verticalLayoutGroup.spacing = 10f;
    }

    private void LoadAndRefreshSlot()
    {
        SaveData saveData = _presenter.LoadSlotData();
        _hasSaveData = saveData is not null;
        RefreshRegionNameText(saveData);
    }

    private void RefreshRegionNameText(SaveData saveData)
    {
        _regionNameText.text = _hasSaveData
            ? SaveSlotRegionNameMapper.GetRegionName(saveData.Region)
            : "빈 슬롯";
    }

    // 이어하기 또는 새 게임 버튼 클릭
    private void OnClickActionBtn()
    {
        if (_mode == ESlotPanelMode.NewGame)
        {
            OnClickNewGameBtn();
        }
        else
        {
            OnClickContinueBtn();
        }
    }

    private void OnClickContinueBtn()
    {
        if (!_hasSaveData)
        {
            return;
        }
        _presenter.TryContinue();
    }

    private void OnClickNewGameBtn()
    {
        NewGameAsync().Forget();
    }

    // 진행 여부 판단과 매니저 오케스트레이션은 프레젠터가 담당한다.
    private async UniTaskVoid NewGameAsync()
    {
        try
        {
            await _presenter.TryStartNewGameAsync(_hasSaveData, this.GetCancellationTokenOnDestroy());
        }
        catch (OperationCanceledException)
        {
            // 확인 팝업 대기 중 슬롯이 파괴된 경우다. 새 게임을 시작하지 않는다.
        }
    }

    private void OnClickDeleteBtn()
    {
        _presenter.DeleteSlot();
        _hasSaveData = false;
        RefreshRegionNameText(null);

        // 버튼 활성과 포커스는 선택 규칙(SetSelected)으로 다시 계산한다. 새 게임 모드라면 빈 슬롯에서도
        // 바로 새 게임을 시작할 수 있어야 하고, 지우기 버튼은 지울 데이터가 없어졌으니 꺼진다.
        SetSelected(true);
    }
}
