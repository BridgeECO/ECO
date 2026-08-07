using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_Popup_PauseMenu : UI_Popup
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _settingButton;

    [SerializeField]
    private Button _titleButton;

    [SerializeField]
    private Button _exitButton;

    // 같은 캔버스의 형제 팝업. UIManager 프로퍼티 경유 대신 직접 바인딩한다.
    [SerializeField]
    private UI_Popup_Settings _settingsPopup;

    [SerializeField]
    private UI_Popup_ExitConfirm _exitConfirmPopup;

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
        if (_settingsPopup != null)
        {
            Handler?.OpenPopup(_settingsPopup);
        }
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
        if (_exitConfirmPopup != null)
        {
            _exitConfirmPopup.Show();
        }
    }
}
