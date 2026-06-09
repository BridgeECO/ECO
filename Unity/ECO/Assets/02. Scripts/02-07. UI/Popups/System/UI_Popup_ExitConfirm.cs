using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_Popup_ExitConfirm : UI_SystemPopup
{
    public enum EResult
    {
        ExitGame,
        Cancel
    }

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _buttonExitGame;


    [SerializeField]
    private Button _buttonCancel;

    private UniTaskCompletionSource<EResult> _tcs;

    public async UniTask<EResult> ShowPopupAsync(string title, string message)
    {
        SetPopupText(title, message);

        _buttonExitGame.onClick.RemoveAllListeners();
        _buttonCancel.onClick.RemoveAllListeners();

        _buttonExitGame.onClick.AddListener(() => OnClick_Button(EResult.ExitGame));
        _buttonCancel.onClick.AddListener(() => OnClick_Button(EResult.Cancel));

        UIManager.Instance.PopupHandler.OpenPopup(this);

        _tcs = new UniTaskCompletionSource<EResult>();
        return await _tcs.Task;
    }

    private void OnClick_Button(EResult result)
    {
        UIManager.Instance.PopupHandler.ClosePopup(this);

        if (_tcs != null)
        {
            _tcs.TrySetResult(result);
            _tcs = null;
        }
    }
}
