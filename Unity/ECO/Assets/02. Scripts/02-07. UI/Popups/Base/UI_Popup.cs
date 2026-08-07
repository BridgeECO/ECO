using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class UI_Popup : MonoBehaviour
{
    public bool IsClosing { get; private set; }
    // 팝업을 여닫는 핸들러. UIManager(등록 팝업) 또는 UI_PopupHandler(열릴 때)가 주입한다.
    // 팝업이 UIManager 싱글턴을 역참조하지 않기 위한 통로다.
    protected UI_PopupHandler Handler { get; private set; }

    // 개폐 스케일 연출. 컴포넌트 탐색(TryGetComponent) 대신 인라인으로 소유한다.
    [SerializeField]
    private UI_ScaleAnimator _scaleAnimation = new UI_ScaleAnimator();

    private UI_SpriteAnimator[] _spriteAnimators;

    protected virtual void Awake()
    {
        _spriteAnimators = GetComponentsInChildren<UI_SpriteAnimator>(true);
    }

    public void SetHandler(UI_PopupHandler handler)
    {
        Handler = handler;
    }

    public virtual void InitPopup()
    {
    }

    public virtual async UniTask OpenAsync()
    {
        gameObject.SetActive(true);
        await _scaleAnimation.PlayOpenAsync(transform, this.GetCancellationTokenOnDestroy());
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
            var tasks = new List<UniTask>();
            foreach (var spriteAnimator in _spriteAnimators)
            {
                if (spriteAnimator != null && spriteAnimator.isActiveAndEnabled)
                {
                    tasks.Add(spriteAnimator.PlayReverseAsync(this.GetCancellationTokenOnDestroy()));
                }
            }

            if (0 < tasks.Count)
            {
                await UniTask.WhenAll(tasks);
            }
        }

        if (this == null)
        {
            return;
        }

        await _scaleAnimation.PlayCloseAsync(transform, this.GetCancellationTokenOnDestroy());

        if (this == null)
        {
            return;
        }

        gameObject.SetActive(false);
        IsClosing = false;

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnCloseButtonClicked()
    {
        Handler?.ClosePopup(this);
    }
}
