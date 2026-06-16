using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_Popup_NewGameConfirm : UI_SystemPopup
{
    public enum EPopupResult
    {
        Confirm,
        Cancel
    }

    [Foldout("Buttons")]
    [SerializeField]
    private Button _buttonConfirm;

    [SerializeField]
    private Button _buttonCancel;

    protected override List<Button> GetButtons() => new List<Button> { _buttonConfirm, _buttonCancel };

    private UniTaskCompletionSource<EPopupResult> _tcs;

    public async UniTask<EPopupResult> ShowPopupAsync()
    {
        ClearAllButtonListeners();

        if (_buttonConfirm != null)
        {
            _buttonConfirm.onClick.AddListener(() => OnClick_Button(EPopupResult.Confirm));
        }

        if (_buttonCancel != null)
        {
            _buttonCancel.onClick.AddListener(() => OnClick_Button(EPopupResult.Cancel));
        }

        UIManager.Instance.PopupHandler.OpenPopup(this);

        _tcs = new UniTaskCompletionSource<EPopupResult>();

        return await _tcs.Task;
    }

    private void OnClick_Button(EPopupResult result)
    {
        UIManager.Instance.PopupHandler.ClosePopup(this);

        if (_tcs != null)
        {
            _tcs.TrySetResult(result);
            _tcs = null;
        }
    }
}
