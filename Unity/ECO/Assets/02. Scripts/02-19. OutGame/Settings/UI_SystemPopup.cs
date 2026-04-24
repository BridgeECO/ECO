using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;
using Ricimi;

public class UI_SystemPopup : Popup
{
    public enum EPopupResult
    {
        Confirm,
        Cancel,
        Ignore
    }

    [Foldout("UI")]
    [SerializeField] private TextMeshProUGUI UI_TitleText;
    [SerializeField] private TextMeshProUGUI UI_MessageText;
    [SerializeField] private Button UI_ConfirmButton;
    [SerializeField] private Button UI_CancelButton;
    [SerializeField] private Button UI_IgnoreButton; 
    [SerializeField] private Image UI_BackgroundOverlay;

    private UniTaskCompletionSource<EPopupResult> tcs;

    private void OnClick_Button(EPopupResult result)
    {
        if (tcs != null)
        {
            tcs.TrySetResult(result);
            tcs = null;
        }

        Close();
    }

    public async UniTask<EPopupResult> ShowPopupAsync(string title, string message, bool useIgnore = false)
    {
        UI_TitleText.text = title;
        UI_MessageText.text = message;

        UI_ConfirmButton.onClick.RemoveAllListeners();
        UI_CancelButton.onClick.RemoveAllListeners();
        
        if (UI_IgnoreButton != null)
        {
            UI_IgnoreButton.onClick.RemoveAllListeners();
        }

        UI_ConfirmButton.onClick.AddListener(() => OnClick_Button(EPopupResult.Confirm));
        UI_CancelButton.onClick.AddListener(() => OnClick_Button(EPopupResult.Cancel));
        
        if (UI_IgnoreButton != null)
        {
            UI_IgnoreButton.gameObject.SetActive(useIgnore);
            if (useIgnore)
            {
                UI_IgnoreButton.onClick.AddListener(() => OnClick_Button(EPopupResult.Ignore));
            }
        }

        gameObject.SetActive(true);
        CustomOpen();

        tcs = new UniTaskCompletionSource<EPopupResult>();
        
        return await tcs.Task;
    }

    public new void Close()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
        {
            animator.Play("Close");
        }

        if (UI_BackgroundOverlay != null)
        {
            UI_BackgroundOverlay.CrossFadeAlpha(0.0f, 0.2f, false);
        }

        WaitAndDisableAsync().Forget();
    }

    private void CustomOpen()
    {
        if (UI_BackgroundOverlay != null)
        {
            UI_BackgroundOverlay.gameObject.SetActive(true);
            UI_BackgroundOverlay.canvasRenderer.SetAlpha(0.0f);
            UI_BackgroundOverlay.CrossFadeAlpha(1.0f, 0.4f, false);
        }
    }

    private async UniTaskVoid WaitAndDisableAsync()
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(destroyTime));
        gameObject.SetActive(false);
    }
}
