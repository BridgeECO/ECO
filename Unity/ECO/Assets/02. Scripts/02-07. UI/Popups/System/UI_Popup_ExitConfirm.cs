using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_Popup_ExitConfirm : UI_Popup_Confirm2Buttons
{
    [Foldout("Buttons")]
    [SerializeField] 
    private Button _buttonExitGame;
    
    [SerializeField] 
    private Button _buttonCancel;

    protected override Button ConfirmButton => _buttonExitGame;
    protected override Button CancelButton => _buttonCancel;

    public override void OnConfirm()
    {
        base.OnConfirm();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
