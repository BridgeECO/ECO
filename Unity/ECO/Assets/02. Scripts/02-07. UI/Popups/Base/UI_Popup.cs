using System;
using System.Collections.Generic;
using System.Threading;
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

    // 개폐 연출을 자기가 모는 자식들. 팝업 Hide보다 먼저 물러나야 해서 따로 들고 있는다.
    // 파생 팝업이 base.Awake를 빠뜨려도 CloseAsync가 죽지 않도록 빈 배열로 시작한다.
    private UI_Reactor[] _childReactors = Array.Empty<UI_Reactor>();

    private readonly List<UniTask> _exitTasks = new List<UniTask>();

    protected virtual void Awake()
    {
        _childReactors = CollectChildReactors();
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

        // 자식 연출 되감기는 어느 경로든 먼저 끝내야 한다. 한 신호 안의 리액션은 병렬로 돌기 때문에,
        // 팝업 Hide와 같은 신호에 실으면 개폐 연출과 겹친다.
        await PlayChildrenExitAsync();

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

    // 자기 Reactor는 뺀다. 팝업 개폐는 OpenAsync/CloseAsync가 직접 몰기 때문에 여기서 겹쳐 부르면 안 된다.
    private UI_Reactor[] CollectChildReactors()
    {
        UI_Reactor[] found = GetComponentsInChildren<UI_Reactor>(true);

        int count = 0;
        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] != _reactor)
            {
                found[count++] = found[i];
            }
        }

        if (count == found.Length)
        {
            return found;
        }

        UI_Reactor[] children = new UI_Reactor[count];
        Array.Copy(found, children, count);
        return children;
    }

    private async UniTask PlayChildrenExitAsync()
    {
        _exitTasks.Clear();
        CancellationToken token = this.GetCancellationTokenOnDestroy();

        // Show를 되감는다. 같은 리액션 인스턴스가 처리하므로 아직 돌고 있던 정재생은 자동으로 끊긴다.
        for (int i = 0; i < _childReactors.Length; i++)
        {
            if (_childReactors[i] != null)
            {
                _exitTasks.Add(_childReactors[i].PlaySignalExitAsync(EUIReactionSignal.Show, token));
            }
        }

        if (0 < _exitTasks.Count)
        {
            await UniTask.WhenAll(_exitTasks);
        }
    }
}
