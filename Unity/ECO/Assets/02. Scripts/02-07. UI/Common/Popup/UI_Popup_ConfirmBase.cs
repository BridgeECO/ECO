using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_Popup_ConfirmBase : UI_SystemPopup
{
    public enum EPopupResult
    {
        Confirm,
        Cancel,
        Ignore
    }

    [Foldout("Hierarchy")]
    [SerializeField]

    private Button _button1;

    [SerializeField]

    private Button _button2;

    [SerializeField]

    private Button _button3;

    private UniTaskCompletionSource<EPopupResult> _tcs;

    public async UniTask<EPopupResult> ShowPopupAsync(string title, string message, bool useIgnore = false)
    {
        SetPopupText(title, message);

        _button1.onClick.RemoveAllListeners();
        _button2.onClick.RemoveAllListeners();


        if (_button3 != null)
        {
            _button3.onClick.RemoveAllListeners();
        }

        _button1.onClick.AddListener(() => OnClick_Button(EPopupResult.Confirm));
        _button2.onClick.AddListener(() => OnClick_Button(EPopupResult.Cancel));


        if (_button3 != null)
        {
            _button3.gameObject.SetActive(useIgnore);
            if (useIgnore)
            {
                _button3.onClick.AddListener(() => OnClick_Button(EPopupResult.Ignore));
            }
        }

        UIManager.Instance.PopupHandler.OpenPopup(this);

        _tcs = new UniTaskCompletionSource<EPopupResult>();


        return await _tcs.Task;
    }

    private void OnClick_Button(EPopupResult result)
    {
        if (_tcs != null)
        {
            _tcs.TrySetResult(result);
            _tcs = null;
        }

        UIManager.Instance.PopupHandler.ClosePopup(this);
    }
}
