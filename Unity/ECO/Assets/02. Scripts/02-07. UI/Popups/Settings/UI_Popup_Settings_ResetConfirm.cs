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

    // 같은 캔버스의 형제 팝업. UIManager 프로퍼티 경유 대신 직접 바인딩한다.
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_Popup_Settings _settingsPopup;

    protected override Button ConfirmButton => _buttonResetAndClose;
    protected override Button CancelButton => _buttonClose;

    protected override void OnConfirm()
    {
        base.OnConfirm();
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
