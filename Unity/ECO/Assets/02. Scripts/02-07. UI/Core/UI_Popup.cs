using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class UI_Popup : MonoBehaviour
{
    [SerializeField]
    private float _destroyTime = 0.5f;

    public float DestroyTime => _destroyTime;

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
        if (TryGetComponent<UI_ScaleAnimator>(out var animator))
        {
            await animator.PlayCloseAsync(this.GetCancellationTokenOnDestroy());
        }
        gameObject.SetActive(false);
    }

    public void OnCloseButtonClicked()
    {
        UIManager.Instance.PopupHandler.ClosePopup(this);
    }
}
