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
    }

    public override async UniTask CloseAsync()
    {
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
        UIManager.Instance.OpenSettingsPopup();
    }

    private void OnClickTitleBtn()
    {
        Time.timeScale = 1f;
        SceneTransitionManager.Instance.TransitionToNewRegionAsync(ESceneNames.TitleScene).Forget();
    }

    private void OnClickExitBtn()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
