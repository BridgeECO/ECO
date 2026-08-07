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
    private UI_SaveSlotPopup _saveSlotPanel;

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
        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Common_Button);
        _saveSlotPanel.SetMode(ESlotPanelMode.NewGame);
        UIManager.Instance.PopupHandler.OpenPopup(_saveSlotPanel);
    }

    private void OnClickContinueBtn()
    {
        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Common_Button);
        _saveSlotPanel.SetMode(ESlotPanelMode.Continue);
        UIManager.Instance.PopupHandler.OpenPopup(_saveSlotPanel);
    }

    private void OnClickSettingBtn()
    {
        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Common_Button);
        UIManager.Instance.OpenSettingsPopup();
    }

    private void OnClickExitBtn()
    {
        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Title_EndProgram);
        UIManager.Instance.OpenExitConfirmPopup();
    }
}
