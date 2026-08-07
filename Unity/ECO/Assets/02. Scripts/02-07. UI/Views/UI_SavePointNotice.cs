using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 세이브 포인트가 갱신되는 순간 알림 문구를 잠깐 띄웠다 지우는 UI.
/// 저장이 실제로 기록된 경우에만 통지받으므로, 같은 세이브 포인트를 다시 지날 때는 뜨지 않는다.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UI_SavePointNotice : MonoBehaviour
{
    [Foldout("Settings")]
    [SerializeField]
    private float _fadeInDuration = 0.25f;

    [SerializeField]
    private float _visibleDuration = 1f;

    [SerializeField]
    private float _fadeOutDuration = 0.25f;

    private CanvasGroup _canvasGroup;
    private CancellationTokenSource _noticeCts;
    private bool _isListenerAdded;

    #region Unity Lifecycle Methods
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        SetAlpha(0f);
        AddEventListeners();
    }

    // 이 UI가 속한 씬이 EventManager가 있는 PersistentScene보다 먼저 로드되면
    // OnEnable 시점에는 EventManager가 아직 없다. 모든 Awake가 끝난 Start에서 한 번 더 시도한다.
    private void Start()
    {
        AddEventListeners();
    }

    private void OnDisable()
    {
        CancelNotice();
        RemoveEventListeners();
    }
    #endregion

    #region Event Handlers
    // 세이브 포인트를 연달아 지나면 재생 중인 알림을 끊고 처음부터 다시 보여준다.
    private void OnSavePointUpdated()
    {
        CancelNotice();
        _noticeCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        PlayNoticeAsync(_noticeCts.Token).Forget();
    }
    #endregion

    #region Animation
    private async UniTaskVoid PlayNoticeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await FadeAsync(0f, 1f, _fadeInDuration, cancellationToken);
            await UniTask.Delay(TimeSpan.FromSeconds(_visibleDuration), ignoreTimeScale: true, cancellationToken: cancellationToken);
            await FadeAsync(1f, 0f, _fadeOutDuration, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // 알림이 겹쳐 취소되었거나 오브젝트가 파괴된 경우다.
            // 알파는 다음 재생이 0f부터 다시 지정하므로 여기서 되돌리지 않는다.
        }
    }

    // 일시정지 중에도 알림이 멈추지 않도록 unscaled 시간으로 보간한다.
    private async UniTask FadeAsync(float fromAlpha, float toAlpha, float duration, CancellationToken cancellationToken)
    {
        if (duration <= 0f)
        {
            SetAlpha(toAlpha);
            return;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SetAlpha(Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration));
            elapsed += Time.unscaledDeltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }
        SetAlpha(toAlpha);
    }
    #endregion

    #region Logic
    private void SetAlpha(float alpha)
    {
        if (_canvasGroup == null)
        {
            return;
        }
        _canvasGroup.alpha = alpha;
    }

    private void CancelNotice()
    {
        if (_noticeCts == null)
        {
            return;
        }

        _noticeCts.Cancel();
        _noticeCts.Dispose();
        _noticeCts = null;
    }

    // OnEnable과 Start 양쪽에서 호출되므로 중복 등록을 플래그로 막는다.
    private void AddEventListeners()
    {
        if (_isListenerAdded || EventManager.Instance == null)
        {
            return;
        }

        EventManager.Instance.AddEventListener(EEventType.SavePointUpdated, OnSavePointUpdated);
        _isListenerAdded = true;
    }

    private void RemoveEventListeners()
    {
        if (!_isListenerAdded)
        {
            return;
        }

        _isListenerAdded = false;
        if (MonoBehaviourSingleton<EventManager>.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.SavePointUpdated, OnSavePointUpdated);
        }
    }
    #endregion
}
