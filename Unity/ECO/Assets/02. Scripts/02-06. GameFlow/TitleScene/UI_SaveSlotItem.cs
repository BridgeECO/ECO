using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 저장 슬롯 하나의 시각적 표현과 버튼 인터랙션을 담당한다.
/// 선택 상태 변경 시 슬롯 이미지가 아래로 내려가며 이어하기/지우기 버튼이 나타난다.
/// </summary>
public class UI_SaveSlotItem : MonoBehaviour
{
    // 선택 시 슬롯이 내려가는 Y 오프셋 (px)
    private const float SELECTED_OFFSET_Y = -80f;
    private const float ANIMATION_DURATION = 0.25f;

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
    private Button _deleteButton;

    private int _slotIndex;
    private bool _hasSaveData;

    private void Awake()
    {
        _continueButton.onClick.AddListener(OnClickContinueBtn);
        _deleteButton.onClick.AddListener(OnClickDeleteBtn);

        // EventSystem 자동 네비게이션 끄기 (수동 제어)
        Navigation noneNav = new Navigation { mode = Navigation.Mode.None };
        if (_continueButton != null) _continueButton.navigation = noneNav;
        if (_deleteButton != null) _deleteButton.navigation = noneNav;
    }

    private void OnDestroy()
    {
        if (_continueButton != null) _continueButton.onClick.RemoveListener(OnClickContinueBtn);
        if (_deleteButton != null) _deleteButton.onClick.RemoveListener(OnClickDeleteBtn);
    }

    public void Init(int slotIndex)
    {
        _slotIndex = slotIndex;
        if (_slotNumberText != null)
        {
            _slotNumberText.text = (slotIndex + 1).ToString();
        }

        LoadAndRefreshSlot();
        ResetToDeselectedState();
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            _buttonContinueAndDelete.SetActive(true);
            _verticalLayoutGroup.spacing = 30f;
            
            // 빈 슬롯이면 버튼들 비활성화
            if (_continueButton != null) _continueButton.interactable = _hasSaveData;
            if (_deleteButton != null) _deleteButton.interactable = _hasSaveData;

            // 선택될 때 첫 번째 상호작용 가능한 버튼에 포커스
            if (_hasSaveData && _continueButton != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(_continueButton.gameObject);
            }
            else
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
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
        if (!_hasSaveData) return;

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
        if (!_hasSaveData) return;

        GameObject currentSelected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (currentSelected == _continueButton.gameObject)
        {
            OnClickContinueBtn();
        }
        else if (currentSelected == _deleteButton.gameObject)
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
        SaveData saveData = SaveManager.Instance.Load(_slotIndex);
        _hasSaveData = saveData != null;
        RefreshRegionNameText(saveData);
    }

    private void RefreshRegionNameText(SaveData saveData)
    {
        if (_regionNameText == null) return;

        if (_hasSaveData)
        {
            _regionNameText.text = SaveSlotRegionNameMapper.GetRegionName(saveData.Region);
        }
        else
        {
            _regionNameText.text = "빈 슬롯";
        }
    }

    private void OnClickContinueBtn()
    {
        if (!_hasSaveData)
        {
            return;
        }

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

        if (_continueButton != null)
        {
            _continueButton.interactable = false;
        }
        if (_deleteButton != null)
        {
            _deleteButton.interactable = false;
        }

        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        
        await UniTask.Delay(300, cancellationToken: ct);
    }
}
