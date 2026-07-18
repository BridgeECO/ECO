using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_LifeDisplay : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<Image> _lifeIcons;

    [Foldout("Project")]
    [SerializeField]
    private List<Sprite> _heartSprites; // 0f(풀) ~ 5f(깨짐) 순서

    [Foldout("Settings")]
    [SerializeField]
    private float _frameInterval = 0.0333f;

    private int _previousLife;
    private CancellationTokenSource[] _iconAnimCts;

    #region Unity Lifecycle Methods
    private void OnEnable()
    {
        _previousLife = LifeManager.Instance.CurrentLife;
        InitIconAnimCts();

        if (EventManager.Instance != null)
        {
            EventManager.Instance.AddEventListener(EEventType.LifeChanged, OnLifeChanged);
        }

        RefreshLifeDisplay();
    }

    private void OnDisable()
    {
        CancelAllAnimations();
        RemoveEventListeners();
    }
    #endregion

    #region Event Handlers
    private void OnLifeChanged()
    {
        int currentLife = LifeManager.Instance.CurrentLife;
        int previousLife = _previousLife;
        _previousLife = currentLife;

        if (currentLife < previousLife)
        {
            // 감소: 해당 슬롯 0f→5f 순방향 재생 후 아이콘 비활성화
            PlayLoseLifeAnimation(previousLife - 1, gameObject.GetCancellationTokenOnDestroy());
        }
        else if (currentLife > previousLife)
        {
            // 한 번에 여러 칸이 늘어나는 경우(부활 등) 중간 슬롯은 즉시 활성화
            for (int i = previousLife; i < currentLife - 1; i++)
            {
                ActivateIconImmediate(i);
            }
            // 최상위 슬롯만 애니메이션으로 활성화
            PlayGainLifeAnimation(currentLife - 1, gameObject.GetCancellationTokenOnDestroy());
        }
    }
    #endregion

    #region Animation
    private async void PlayLoseLifeAnimation(int iconIndex, CancellationToken externalToken)
    {
        if (!IsValidIconIndex(iconIndex))
        {
            RefreshLifeDisplay();
            return;
        }

        using var linked = CreateLinkedCts(iconIndex, externalToken);

        await PlaySpritesForwardAsync(_lifeIcons[iconIndex], linked.Token);

        if (!linked.Token.IsCancellationRequested)
        {
            RefreshLifeDisplay();
        }
    }

    private async void PlayGainLifeAnimation(int iconIndex, CancellationToken externalToken)
    {
        if (!IsValidIconIndex(iconIndex))
        {
            RefreshLifeDisplay();
            return;
        }

        _lifeIcons[iconIndex].enabled = true;

        using var linked = CreateLinkedCts(iconIndex, externalToken);

        await PlaySpritesReverseAsync(_lifeIcons[iconIndex], linked.Token);
    }

    private async UniTask PlaySpritesForwardAsync(Image target, CancellationToken ct)
    {
        if (_heartSprites == null || _heartSprites.Count == 0)
        {
            return;
        }

        for (int i = 0; i < _heartSprites.Count; i++)
        {
            if (target == null)
            {
                return;
            }

            target.sprite = _heartSprites[i];

            if (i < _heartSprites.Count - 1)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_frameInterval), ignoreTimeScale: true, cancellationToken: ct);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    private async UniTask PlaySpritesReverseAsync(Image target, CancellationToken ct)
    {
        if (_heartSprites == null || _heartSprites.Count == 0)
        {
            return;
        }

        for (int i = _heartSprites.Count - 1; 0 <= i; i--)
        {
            if (target == null)
            {
                return;
            }

            target.sprite = _heartSprites[i];

            if (0 < i)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_frameInterval), ignoreTimeScale: true, cancellationToken: ct);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }
    #endregion

    #region Logic
    private void RefreshLifeDisplay()
    {
        int currentLife = LifeManager.Instance.CurrentLife;
        for (int i = 0; i < _lifeIcons.Count; i++)
        {
            if (_lifeIcons[i] == null)
            {
                continue;
            }

            bool isActive = i < currentLife;
            _lifeIcons[i].enabled = isActive;

            // 활성 아이콘은 0f(풀 하트) 스프라이트로 초기화
            if (isActive && _heartSprites != null && _heartSprites.Count > 0)
            {
                _lifeIcons[i].sprite = _heartSprites[0];
            }
        }
    }

    private void InitIconAnimCts()
    {
        CancelAllAnimations();
        _iconAnimCts = new CancellationTokenSource[_lifeIcons.Count];
        for (int i = 0; i < _iconAnimCts.Length; i++)
        {
            _iconAnimCts[i] = new CancellationTokenSource();
        }
    }

    // 해당 슬롯의 이전 애니메이션을 취소하고 새 CTS를 외부 토큰과 연결해 반환
    private CancellationTokenSource CreateLinkedCts(int iconIndex, CancellationToken externalToken)
    {
        if (_iconAnimCts[iconIndex] != null)
        {
            _iconAnimCts[iconIndex].Cancel();
            _iconAnimCts[iconIndex].Dispose();
        }
        _iconAnimCts[iconIndex] = new CancellationTokenSource();
        return CancellationTokenSource.CreateLinkedTokenSource(externalToken, _iconAnimCts[iconIndex].Token);
    }

    private void CancelAllAnimations()
    {
        if (_iconAnimCts == null)
        {
            return;
        }

        for (int i = 0; i < _iconAnimCts.Length; i++)
        {
            if (_iconAnimCts[i] == null)
            {
                continue;
            }

            _iconAnimCts[i].Cancel();
            _iconAnimCts[i].Dispose();
            _iconAnimCts[i] = null;
        }
    }

    private bool IsValidIconIndex(int index)
    {
        return _lifeIcons != null
            && 0 <= index
            && index < _lifeIcons.Count
            && _lifeIcons[index] != null;
    }

    private void ActivateIconImmediate(int iconIndex)
    {
        if (!IsValidIconIndex(iconIndex))
        {
            return;
        }

        _lifeIcons[iconIndex].enabled = true;

        if (_heartSprites != null && _heartSprites.Count > 0)
        {
            _lifeIcons[iconIndex].sprite = _heartSprites[0];
        }
    }

    private void RemoveEventListeners()
    {
        if (MonoBehaviourSingleton<EventManager>.HasInstance)
        {
            EventManager.Instance.RemoveEventListener(EEventType.LifeChanged, OnLifeChanged);
        }
    }
    #endregion
}
