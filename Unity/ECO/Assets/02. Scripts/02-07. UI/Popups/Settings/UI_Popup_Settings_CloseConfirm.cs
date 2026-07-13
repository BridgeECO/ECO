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
}
