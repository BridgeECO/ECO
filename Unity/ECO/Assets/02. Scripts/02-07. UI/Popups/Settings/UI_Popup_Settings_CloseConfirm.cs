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

    protected override Button Action1Button => _buttonSaveAndClose;
    protected override Button Action2Button => _buttonDontSaveAndClose;
    protected override Button CancelButton => _buttonClose;

    private UI_SettingsPopup _settingsPopup;

    public void Init(UI_SettingsPopup settingsPopup)
    {
        _settingsPopup = settingsPopup;
    }

    public override void OnAction1()
    {
        base.OnAction1();
        if (_settingsPopup != null)
        {
            var targetTab = _settingsPopup.ActiveTab;
            if (targetTab != null)
            {
                targetTab.SaveTabSettings();
            }
            UIManager.Instance.PopupHandler.ClosePopup(_settingsPopup);
        }
    }

    public override void OnAction2()
    {
        base.OnAction2();
        if (_settingsPopup != null)
        {
            UIManager.Instance.PopupHandler.ClosePopup(_settingsPopup);
        }
    }
}
