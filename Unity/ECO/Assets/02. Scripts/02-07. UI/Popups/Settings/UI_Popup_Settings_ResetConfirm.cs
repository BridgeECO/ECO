using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_Popup_Settings_ResetConfirm : UI_Popup_Confirm2Buttons
{
    [Foldout("Buttons")]
    [SerializeField] 
    private Button _buttonResetAndClose;
    
    [SerializeField] 
    private Button _buttonClose;

    protected override Button ConfirmButton => _buttonResetAndClose;
    protected override Button CancelButton => _buttonClose;

    protected override void OnConfirm()
    {
        base.OnConfirm();
        var settingsPopup = UIManager.Instance.SettingsPopup;
        var targetTab = settingsPopup.ActiveTab;
        var feedback = settingsPopup.SettingsFeedback;
        if (targetTab != null)
        {
            targetTab.ResetTabToDefault();
            if (feedback != null)
            {
                feedback.Play("설정값이 초기화되었습니다.");
            }
        }
    }
}
