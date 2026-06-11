using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class UI_Popup : MonoBehaviour
{

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
        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        var tasks = new List<UniTask>();

        if (_spriteAnimators != null && _spriteAnimators.Length > 0)
        {
            foreach (var spriteAnimator in _spriteAnimators)
            {
                if (spriteAnimator != null && spriteAnimator.gameObject.activeInHierarchy)
                {
                    tasks.Add(spriteAnimator.PlayReverseAsync(this.GetCancellationTokenOnDestroy()));
                }
            }
        }

        if (TryGetComponent<UI_ScaleAnimator>(out var animator))
        {
            tasks.Add(animator.PlayCloseAsync(this.GetCancellationTokenOnDestroy()));
        }

        if (tasks.Count > 0)
        {
            await UniTask.WhenAll(tasks);
        }

        gameObject.SetActive(false);

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnCloseButtonClicked()
    {
        UIManager.Instance.PopupHandler.ClosePopup(this);
    }
}
