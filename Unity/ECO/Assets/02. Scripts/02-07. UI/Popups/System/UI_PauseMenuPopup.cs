using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_PauseMenuPopup : UI_Popup
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _settingButton;

    [SerializeField]
    private Button _titleButton;

    [SerializeField]
    private Button _exitButton;

    public override void InitPopup()
    {
        base.InitPopup();
    }

    public override void Open()
    {
        base.Open();
        Time.timeScale = 0f;
    }

    public override void Close()
    {
        base.Close();
        Time.timeScale = 1f;
    }

    private void Awake()
    {
        _settingButton.onClick.AddListener(OnClickSettingBtn);
        _titleButton.onClick.AddListener(OnClickTitleBtn);
        _exitButton.onClick.AddListener(OnClickExitBtn);
    }

    private void OnDestroy()
    {
        _settingButton.onClick.RemoveListener(OnClickSettingBtn);
        _titleButton.onClick.RemoveListener(OnClickTitleBtn);
        _exitButton.onClick.RemoveListener(OnClickExitBtn);
    }

    private void OnClickSettingBtn()
    {
        UIManager.Instance.OpenSettingsPopup();
    }

    private void OnClickTitleBtn()
    {
        Time.timeScale = 1f;
        SceneTransitionManager.Instance.TransitionToNewRegionAsync(ESceneNames.TitleScene).Forget();
    }

    private void OnClickExitBtn()
    {
        Application.Quit();
    }
}
