using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 저장 슬롯 하나의 시각적 표현과 버튼 인터랙션을 담당한다.
/// 선택 상태 변경 시 이어하기/선택 버튼과 지우기 버튼이 나타난다.
/// Init(slotIndex, mode)로 패널 모드를 주입받아 버튼 동작을 분기한다.
/// </summary>
public class UI_SaveSlotItem : MonoBehaviour
{
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

    private int _slotIndex;
    private bool _hasSaveData;
    private ESlotPanelMode _mode;

    private void Awake()
    {
        _continueButton.onClick.AddListener(OnClickActionBtn);
        _deleteButton.onClick.AddListener(OnClickDeleteBtn);

        // EventSystem 자동 네비게이션 끄기 (수동 제어)
        Navigation noneNav = new Navigation { mode = Navigation.Mode.None };
        _continueButton.navigation = noneNav;
        _deleteButton.navigation = noneNav;
    }

    private void OnDestroy()
    {
        if (_continueButton != null) { _continueButton.onClick.RemoveListener(OnClickActionBtn); }
        if (_deleteButton != null) { _deleteButton.onClick.RemoveListener(OnClickDeleteBtn); }
    }

    public void Init(int slotIndex, ESlotPanelMode mode)
    {
        _slotIndex = slotIndex;
        _mode = mode;

        if (_slotNumberText != null)
        {
            _slotNumberText.text = (slotIndex + 1).ToString();
        }

        RefreshButtonLabel();
        LoadAndRefreshSlot();
        ResetToDeselectedState();
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            _buttonContinueAndDelete.SetActive(true);
            _verticalLayoutGroup.spacing = 30f;

            // Continue 모드: 저장 데이터가 있을 때만 이어하기 활성화
            // NewGame 모드: 항상 슬롯 선택 가능 (빈 슬롯도 선택 가능)
            bool canAction = _mode == ESlotPanelMode.NewGame || _hasSaveData;
            _continueButton.interactable = canAction;
            _deleteButton.interactable = _hasSaveData;

            FocusFirstInteractableButton(canAction);
        }
        else
        {
            _buttonContinueAndDelete.SetActive(false);
            _verticalLayoutGroup.spacing = 10f;
        }
    }

    // 외부에서 상하 방향키 입력 시 호출
    public void MoveButtonSelection(int direction)
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            return;
        }

        GameObject currentSelected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (currentSelected == _continueButton.gameObject && direction == 1) // Down
        {
            if (_deleteButton != null && _deleteButton.interactable)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(_deleteButton.gameObject);
            }
        }
        else if (currentSelected == _deleteButton.gameObject && direction == -1) // Up
        {
            if (_continueButton != null && _continueButton.interactable)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(_continueButton.gameObject);
            }
        }
    }

    // 외부에서 엔터/스페이스바 입력 시 호출
    public void PressSelectedButton()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            return;
        }

        GameObject currentSelected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (currentSelected == _continueButton.gameObject)
        {
            OnClickActionBtn();
        }
        else if (currentSelected == _deleteButton.gameObject)
        {
            OnClickDeleteBtn();
        }
    }

    private void RefreshButtonLabel()
    {
        if (_continueButtonText == null)
        {
            return;
        }
        _continueButtonText.text = _mode == ESlotPanelMode.NewGame ? "새 게임" : "이어하기";
    }

    private void FocusFirstInteractableButton(bool canAction)
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            return;
        }

        if (canAction && _continueButton != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(_continueButton.gameObject);
        }
        else
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void ResetToDeselectedState()
    {
        _buttonContinueAndDelete.SetActive(false);
        _verticalLayoutGroup.spacing = 10f;
    }

    private void LoadAndRefreshSlot()
    {
        SaveData saveData = SaveManager.Instance.Load(_slotIndex);
        _hasSaveData = saveData != null;
        RefreshRegionNameText(saveData);
    }

    private void RefreshRegionNameText(SaveData saveData)
    {
        if (_regionNameText == null)
        {
            return;
        }

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

        SaveData saveData = SaveManager.Instance.Load(_slotIndex);
        if (saveData is null)
        {
            return;
        }

        if (!RegionSceneMapper.TryGetSceneName(saveData.Region, out ESceneNames sceneName))
        {
            Debug.LogError($"[UI_SaveSlotItem] {saveData.Region}에 대응하는 씬이 RegionSceneMapper에 없습니다.");
            return;
        }

        SaveManager.Instance.CurrentSlotIndex = _slotIndex;
        SceneTransitionManager.Instance.TransitionToNewRegionAsync(sceneName).Forget();
    }

    private void OnClickNewGameBtn()
    {
        NewGameAsync().Forget();
    }

    /// <summary>
    /// 빈 슬롯이면 바로 새 게임 시작.
    /// 이미 데이터가 있는 슬롯이면 덮어쓰기 경고 팝업을 띄우고 수락 시에만 진행한다.
    /// </summary>
    private async UniTaskVoid NewGameAsync()
    {
        var ct = this.GetCancellationTokenOnDestroy();

        if (_hasSaveData)
        {
            bool confirmed = await ShowOverwriteConfirmPopupAsync(ct);
            if (!confirmed)
            {
                return;
            }
            SaveManager.Instance.DeleteSave(_slotIndex);
        }

        StartNewGame();
    }

    private async UniTask<bool> ShowOverwriteConfirmPopupAsync(System.Threading.CancellationToken ct)
    {
        UI_Popup_NewGameConfirm popup = UIManager.Instance.NewGameConfirmPopup;
        if (popup == null)
        {
            Debug.LogWarning("[UI_SaveSlotItem] NewGameConfirmPopup이 UIManager에 할당되지 않았습니다. 즉시 진행합니다.");
            return true;
        }

        UI_Popup_NewGameConfirm.EPopupResult result = await popup.ShowPopupAsync();

        return result == UI_Popup_NewGameConfirm.EPopupResult.Confirm;
    }

    private void StartNewGame()
    {
        SaveManager.Instance.CurrentSlotIndex = _slotIndex;
        SceneTransitionManager.Instance.TransitionToNewRegionAsync(ESceneNames.CenterRoomScene).Forget();
    }

    private void OnClickDeleteBtn()
    {
        DeleteSlotAsync().Forget();
    }

    private async UniTaskVoid DeleteSlotAsync()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        SaveManager.Instance.DeleteSave(_slotIndex);
        _hasSaveData = false;

        if (_regionNameText != null)
        {
            _regionNameText.text = "빈 슬롯";
        }

        _continueButton.interactable = false;
        _deleteButton.interactable = false;

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        await UniTask.Delay(300, cancellationToken: ct);
    }
}
