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
}
