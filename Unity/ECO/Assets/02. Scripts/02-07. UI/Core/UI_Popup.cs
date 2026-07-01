using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class UI_Popup : MonoBehaviour
{
    public bool IsClosing { get; private set; }

    private UI_SpriteAnimator[] _spriteAnimators;

    protected virtual void Awake()
    {
        _spriteAnimators = GetComponentsInChildren<UI_SpriteAnimator>(true);
    }

    public virtual void InitPopup()
    {
    }

    public virtual async UniTask OpenAsync()
    {
        gameObject.SetActive(true);
        if (TryGetComponent<UI_ScaleAnimator>(out var animator))
        {
            await animator.PlayOpenAsync(this.GetCancellationTokenOnDestroy());
        }
    }

    public virtual async UniTask CloseAsync()
    {
        IsClosing = true;

        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        if (_spriteAnimators != null && 0 < _spriteAnimators.Length)
        {
            foreach (var spriteAnimator in _spriteAnimators)
            {
                if (spriteAnimator != null && spriteAnimator.isActiveAndEnabled)
                {
                    spriteAnimator.PlayReverseAsync(this.GetCancellationTokenOnDestroy()).Forget();
                }
            }
        }

        if (TryGetComponent<UI_ScaleAnimator>(out var animator))
        {
            await animator.PlayCloseAsync(this.GetCancellationTokenOnDestroy());
        }

        gameObject.SetActive(false);
        IsClosing = false;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnCloseButtonClicked()
    {
        UIManager.Instance.PopupHandler.ClosePopup(this);
    }
}
