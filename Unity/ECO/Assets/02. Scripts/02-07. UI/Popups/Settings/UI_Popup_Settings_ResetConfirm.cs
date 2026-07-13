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

    private UI_SettingsPopup _settingsPopup;

    public void Init(UI_SettingsPopup settingsPopup)
    {
        _settingsPopup = settingsPopup;
    }

    public override void OnConfirm()
    {
        base.OnConfirm();
        if (_settingsPopup != null)
        {
            var targetTab = _settingsPopup.ActiveTab;
            var feedback = _settingsPopup.SettingsFeedback;
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
}
