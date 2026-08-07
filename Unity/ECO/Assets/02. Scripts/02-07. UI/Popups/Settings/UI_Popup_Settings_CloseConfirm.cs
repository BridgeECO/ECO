using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_Popup_Settings_CloseConfirm : UI_Popup_Confirm3Buttons
{
    [Foldout("Buttons")]
    [SerializeField] 
    private Button _buttonSaveAndClose;
    
    [SerializeField] 
    private Button _buttonDontSaveAndClose;
    
    [SerializeField]
    private Button _buttonClose;

    // 같은 캔버스의 형제 팝업. UIManager 프로퍼티 경유 대신 직접 바인딩한다.
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_Popup_Settings _settingsPopup;

    protected override Button Action1Button => _buttonSaveAndClose;
    protected override Button Action2Button => _buttonDontSaveAndClose;
    protected override Button CancelButton => _buttonClose;

    protected override void OnAction1()
    {
        base.OnAction1();
        var targetTab = _settingsPopup.ActiveTab;
        if (targetTab != null)
        {
            targetTab.SaveTabSettings();
        }
        Handler?.ClosePopup(_settingsPopup);
    }

    protected override void OnAction2()
    {
        base.OnAction2();
        Handler?.ClosePopup(_settingsPopup);
    }
}
