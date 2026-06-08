using Cysharp.Threading.Tasks;
using Ricimi;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public abstract class UI_SystemPopup : UI_Popup
{
    [Foldout("UI")]
    [SerializeField]
    private TextMeshProUGUI _uiTitleText;

    [SerializeField]
    private TextMeshProUGUI _uiMessageText;

    protected void SetPopupText(string title, string message)
    {
        if (_uiTitleText != null) _uiTitleText.text = title;
        if (_uiMessageText != null) _uiMessageText.text = message;
    }

    public override void Close()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
        {
            animator.Play("Close");
        }

        WaitAndDisableAsync().Forget();
    }

    private async UniTaskVoid WaitAndDisableAsync()
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(DestroyTime));
        gameObject.SetActive(false);
    }
}
