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
    }

    private void OnClickSettingBtn()
    {
    }

    private void OnClickExitBtn()
    {
        Application.Quit();
    }
}
