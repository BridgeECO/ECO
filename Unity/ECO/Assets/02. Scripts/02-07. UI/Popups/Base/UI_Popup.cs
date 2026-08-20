using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class UI_Popup : MonoBehaviour
{
    public bool IsClosing { get; private set; }
    // 팝업을 여닫는 핸들러. UIManager(등록 팝업) 또는 UI_PopupHandler(열릴 때)가 주입한다.
    // 팝업이 UIManager 싱글턴을 역참조하지 않기 위한 통로다.
    protected UI_PopupHandler Handler { get; private set; }

    // 개폐 연출을 UI_Reactor로 옮긴 팝업만 연결한다. 비워 두면 아래 기존 경로를 그대로 탄다.
    // 이 팝업의 Reactor는 개폐를 OpenAsync/CloseAsync가 직접 몰기 때문에 Show 자동 재생을 꺼 둔다.
    [SerializeField]
    private UI_Reactor _reactor;

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

        if (_reactor != null)
        {
            await _reactor.PlaySignalAsync(EUIReactionSignal.Show, this.GetCancellationTokenOnDestroy());
            return;
        }

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

        // 자식 스프라이트 애니메이터 역재생은 어느 경로든 먼저 끝내야 한다.
        // 리액션은 한 신호 안에서 병렬로 도는 탓에, 이걸 Reactor로 옮기면 개폐 연출과 겹쳐 버린다.
        await PlaySpriteAnimatorsReverseAsync();

        if (this == null)
        {
            return;
        }

        if (_reactor != null)
        {
            await _reactor.PlaySignalAsync(EUIReactionSignal.Hide, this.GetCancellationTokenOnDestroy());
        }
        else
        {
            await _scaleAnimation.PlayCloseAsync(transform, this.GetCancellationTokenOnDestroy());
        }

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

    private async UniTask PlaySpriteAnimatorsReverseAsync()
    {
        if (_spriteAnimators == null || _spriteAnimators.Length <= 0)
        {
            return;
        }

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
}
