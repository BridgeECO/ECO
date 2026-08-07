using System.Collections.Generic;
using System.Threading;
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

    public async UniTask<EPopupResult> ShowPopupAsync(CancellationToken cancellationToken)
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

        Handler.OpenPopup(this);

        _tcs = new UniTaskCompletionSource<EPopupResult>();

        return await _tcs.Task.AttachExternalCancellation(cancellationToken);
    }

    // ESC나 ClearAllPopups로 닫히는 경로에서는 버튼 콜백이 돌지 않는다.
    // 대기 중인 호출자가 영영 멈추지 않도록 취소로 간주해 완료시킨다.
    public override async UniTask CloseAsync()
    {
        CompleteWaiting(EPopupResult.Cancel);
        await base.CloseAsync();
    }

    private void OnClick_Button(EPopupResult result)
    {
        // 닫기보다 먼저 결과를 확정해야 CloseAsync의 취소 처리가 결과를 덮어쓰지 않는다.
        CompleteWaiting(result);
        Handler.ClosePopup(this);
    }

    private void CompleteWaiting(EPopupResult result)
    {
        if (_tcs == null)
        {
            return;
        }

        UniTaskCompletionSource<EPopupResult> completionSource = _tcs;
        _tcs = null;
        completionSource.TrySetResult(result);
    }
}

