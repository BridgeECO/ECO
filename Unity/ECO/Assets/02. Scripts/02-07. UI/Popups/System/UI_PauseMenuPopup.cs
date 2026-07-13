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

    public override async UniTask OpenAsync()
    {
        await base.OpenAsync();
        Time.timeScale = 0f;
        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Pause_StartPause);
    }

    public override async UniTask CloseAsync()
    {
        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Pause_EndPause);
        await base.CloseAsync();
        Time.timeScale = 1f;
    }

    protected override void Awake()
    {
        base.Awake();
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
        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Common_Button);
        UIManager.Instance.OpenSettingsPopup();
    }

    private void OnClickTitleBtn()
    {
        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Common_Button);
        Time.timeScale = 1f;
        SceneTransitionManager.Instance.TransitionToNewRegionAsync(ESceneNames.TitleScene).Forget();
    }

    private void OnClickExitBtn()
    {
        SoundManager.Instance.PlayUiSfx(ESfxClip.SUI_Common_Button);
        if (UIManager.Instance.ExitConfirmPopup != null)
        {
            UIManager.Instance.ExitConfirmPopup.Show();
        }
    }
}
