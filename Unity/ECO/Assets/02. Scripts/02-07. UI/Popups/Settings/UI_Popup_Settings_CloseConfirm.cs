using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_Popup_Settings_CloseConfirm : UI_SystemPopup
{
    public enum EResult
    {
        SaveAndClose,
        DontSaveAndClose,
        Cancel
    }

    [Foldout("Buttons")]
    [SerializeField] 
    private Button _buttonSaveAndClose;
    
    [SerializeField] 
    private Button _buttonDontSaveAndClose;
    
    [SerializeField] 
    private Button _buttonClose;

    protected override List<Button> GetButtons() => new List<Button> { _buttonSaveAndClose, _buttonDontSaveAndClose, _buttonClose };

    private UniTaskCompletionSource<EResult> _tcs;

    public async UniTask<EResult> ShowPopupAsync(string title, string message)
    {
        SetPopupText(title, message);
        ClearAllButtonListeners();

        if (_buttonSaveAndClose != null)
        {
            _buttonSaveAndClose.onClick.AddListener(() => OnClick_Button(EResult.SaveAndClose));
        }
        if (_buttonDontSaveAndClose != null)
        {
            _buttonDontSaveAndClose.onClick.AddListener(() => OnClick_Button(EResult.DontSaveAndClose));
        }
        if (_buttonClose != null)
        {
            _buttonClose.onClick.AddListener(() => OnClick_Button(EResult.Cancel));
        }

        UIManager.Instance.PopupHandler.OpenPopup(this);

        _tcs = new UniTaskCompletionSource<EResult>();
        return await _tcs.Task;
    }

    private void OnClick_Button(EResult result)
    {
        UIManager.Instance.PopupHandler.ClosePopup(this);

        if (_tcs is not null)
        {
            _tcs.TrySetResult(result);
            _tcs = null;
        }
    }
}
