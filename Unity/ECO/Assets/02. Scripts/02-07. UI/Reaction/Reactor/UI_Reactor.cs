using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VInspector;

/// <summary>UI 오브젝트에 붙이는 연출 컴포넌트. 기획자가 인스펙터에서 트리거별로 리액션을 조립한다.</summary>
// IDragHandler 계열은 구현하지 않는다. 붙이는 순간 ScrollRect 안의 버튼이 스크롤을 먹는다.
public class UI_Reactor : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,
    IPointerClickHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, ICancelHandler
{
    [Foldout("Settings")]
    [SerializeField]
    private List<UI_ReactionEntry> _entries = new List<UI_ReactionEntry>();

    [SerializeField]
    [Tooltip("활성화될 때 Show 신호를 자동으로 재생합니다. 팝업처럼 호출부가 개폐를 직접 몰면 끕니다.")]
    private bool _isPlayShowOnEnable = true;

    [Foldout("Preview")]
    [SerializeField]
    [Tooltip("플레이 중에만 동작합니다.")]
    private EUIReactionState _previewState = EUIReactionState.Hover;

    private readonly UI_ReactionRuntime _runtime = new UI_ReactionRuntime();

    private Selectable _selectable;
    private CancellationTokenSource _cts;

    public IReadOnlyList<UI_ReactionEntry> Entries => _entries;

    #region Unity Lifecycle Methods
    private void Awake()
    {
        _selectable = GetComponent<Selectable>();
        _runtime.Init(gameObject, _entries);
    }

    private void OnEnable()
    {
        // 파괴가 아니라 비활성화 단위로 끊어야 풀링된 UI가 재사용될 때 이전 작업이 남지 않는다.
        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        _runtime.SetToken(_cts.Token);

        if (_selectable != null && _runtime.HasDisabledEntry)
        {
            UI_ReactorTicker.Register(this);
            _runtime.SetInteractable(_selectable.IsInteractable());
        }

        _runtime.RefreshStates();
        _runtime.WaitLayoutSettledAsync(_cts.Token).Forget();

        if (_isPlayShowOnEnable)
        {
            _runtime.PlaySignalAsync(EUIReactionSignal.Show, _cts.Token).Forget();
        }
    }

    private void OnDisable()
    {
        UI_ReactorTicker.Unregister(this);

        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        _runtime.Deactivate();
    }

    // 부모 CanvasGroup이 상호작용을 막으면 이 메시지가 온다.
    // interactable을 직접 끄는 경로는 콜백이 없어 티커가 폴링으로 따로 잡는다.
    private void OnCanvasGroupChanged()
    {
        TickInteractable();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UI_ReactorValidator.Validate(this);
        UI_ReactionReferenceRepairer.Repair(this);
    }
#endif
    #endregion

    #region Event Handlers
    public void OnPointerEnter(PointerEventData eventData)
    {
        _runtime.SetPointerInside(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _runtime.SetPointerInside(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _runtime.SetPressed(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _runtime.SetPressed(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _runtime.PlayEvent(EUIReactionEvent.Click);
        _runtime.PlayActivate();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        _runtime.PlayEvent(EUIReactionEvent.Submit);
        _runtime.PlayActivate();
    }

    public void OnSelect(BaseEventData eventData)
    {
        _runtime.SetSelected(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _runtime.SetSelected(false);
    }

    public void OnCancel(BaseEventData eventData)
    {
        _runtime.PlayEvent(EUIReactionEvent.Cancel);
    }
    #endregion

    #region Logic
    /// <summary>Hide처럼 끝나야 다음 단계로 넘어가는 연출은 반드시 await한 뒤 비활성화할 것.</summary>
    public UniTask PlaySignalAsync(EUIReactionSignal signal, CancellationToken cancellationToken = default)
    {
        // 꺼져 있으면 재생할 대상이 없다. Hide는 호출부가 끄기 전에 await하는 순서를 지켜야 한다.
        if (_cts == null)
        {
            return UniTask.CompletedTask;
        }

        return _runtime.PlaySignalAsync(signal, cancellationToken);
    }

    /// <summary>재생해 둔 신호를 되감는다. 같은 리액션이 처리하므로 진행 중이던 정재생은 자동으로 끊긴다.</summary>
    public UniTask PlaySignalExitAsync(EUIReactionSignal signal, CancellationToken cancellationToken = default)
    {
        if (_cts == null)
        {
            return UniTask.CompletedTask;
        }

        return _runtime.PlaySignalExitAsync(signal, cancellationToken);
    }

    /// <summary>UI_ReactorTicker가 프레임마다 부른다. 필드 두 개를 읽는 비교라 비용이 거의 없다.</summary>
    public void TickInteractable()
    {
        if (_selectable == null)
        {
            return;
        }

        _runtime.SetInteractable(_selectable.IsInteractable());
    }

    [Button("미리보기 재생")]
    private void PlayPreview()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[UI_Reactor] 미리보기는 플레이 중에만 동작합니다.", this);
            return;
        }

        _runtime.PlayStatePreview(_previewState);
    }

    [Button("기본 상태로")]
    private void ResetPreview()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        _runtime.PlayStatePreview(EUIReactionState.Normal);
    }
    #endregion
}
