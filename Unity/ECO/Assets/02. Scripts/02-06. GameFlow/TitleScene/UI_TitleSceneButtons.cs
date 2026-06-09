using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_TitleSceneButtons : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _startButton;

    [SerializeField]
    private Button _continueButton;

    [SerializeField]
    private Button _settingButton;

    [SerializeField]
    private Button _exitButton;

    [SerializeField]
    private UI_SaveSlotPanel _saveSlotPanel;

    private void Awake()
    {
        _startButton.onClick.AddListener(OnClickStartBtn);
        _continueButton.onClick.AddListener(OnClickContinueBtn);
        _settingButton.onClick.AddListener(OnClickSettingBtn);
        _exitButton.onClick.AddListener(OnClickExitBtn);
    }

    private void OnDestroy()
    {
        _startButton.onClick.RemoveListener(OnClickStartBtn);
        _continueButton.onClick.RemoveListener(OnClickContinueBtn);
        _settingButton.onClick.RemoveListener(OnClickSettingBtn);
        _exitButton.onClick.RemoveListener(OnClickExitBtn);
    }

    private void OnClickStartBtn()
    {
        SceneTransitionManager.Instance.TransitionToNewRegionAsync(ESceneNames.CenterRoomScene).Forget();
    }

    private void OnClickContinueBtn()
    {
        UIManager.Instance.PopupHandler.OpenPopup(_saveSlotPanel);
    }

    private void OnClickSettingBtn()
    {
        UIManager.Instance.OpenSettingsPopup();
    }

    private void OnClickExitBtn()
    {
        HandleExitAsync().Forget();
    }

    private async UniTaskVoid HandleExitAsync()
    {
        bool shouldExit = true;
        if (UIManager.Instance.ExitConfirmPopup != null)
        {
            var result = await UIManager.Instance.ExitConfirmPopup.ShowPopupAsync("게임 종료", "정말 게임을 종료하시겠습니까?");
            shouldExit = (result == UI_Popup_ExitConfirm.EResult.ExitGame);
        }

        if (shouldExit)
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
