using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VInspector;

public class UI_SpriteAnimator : MonoBehaviour
{
    public Action OnAnimationComplete;

    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _targetImage;

    [Foldout("Project")]
    [SerializeField]
    private List<Sprite> _sprites = new List<Sprite>();

    [Foldout("Settings")]
    [SerializeField]
    [FormerlySerializedAs("_frameDelay")]
    private float _frameInterval = 0.0333f;

    [SerializeField]
    private bool _isLoop = true;

    [SerializeField, Range(2f, 10f)]
    private float _loopInterval = 2f;

    private CancellationTokenSource _cts;
    private CancellationTokenSource _reverseCts;

    #region Unity Lifecycle Methods
    private void OnEnable()
    {
        _cts = new CancellationTokenSource();
        PlayAnimationAsync(_cts.Token).Forget();
    }

    private void OnDisable()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        if (_reverseCts != null)
        {
            _reverseCts.Cancel();
            _reverseCts.Dispose();
            _reverseCts = null;
        }
    }
    #endregion

    #region Logic
    private async UniTask PlayAnimationAsync(CancellationToken cancellationToken)
    {
        if (_sprites == null || _sprites.Count == 0 || _targetImage == null)
        {
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            if (_targetImage == null)
            {
                return;
            }

            float cycleStartTime = Time.unscaledTime;

            bool success = await PlaySingleCycleAsync(cancellationToken);
            if (!success)
            {
                return;
            }

            OnAnimationComplete?.Invoke();

            if (!_isLoop)
            {
                break;
            }

            bool delaySuccess = await WaitForNextCycleAsync(cycleStartTime, cancellationToken);
            if (!delaySuccess)
            {
                break;
            }
        }
    }

    private async UniTask<bool> PlaySingleCycleAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i < _sprites.Count; i++)
        {
            if (_targetImage == null)
            {
                return false;
            }
            _targetImage.sprite = _sprites[i];

            if (i < _sprites.Count - 1)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_frameInterval), ignoreTimeScale: true, cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }

                if (_targetImage == null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private async UniTask<bool> WaitForNextCycleAsync(float cycleStartTime, CancellationToken cancellationToken)
    {
        float elapsed = Time.unscaledTime - cycleStartTime;
        float remainingDelay = _loopInterval - elapsed;

        try
        {
            if (0f< remainingDelay )
            {
                await UniTask.Delay(TimeSpan.FromSeconds(remainingDelay), cancellationToken: cancellationToken);
            }
            else
            {
                // _loopInterval이 경과했거나 0 이하인 경우 최소 프레임 딜레이 대기하여 오버헤드 방지
                await UniTask.DelayFrame(1, PlayerLoopTiming.Update, cancellationToken: cancellationToken);
            }
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    public async UniTask PlayReverseAsync(CancellationToken cancellationToken)
    {
        if (_sprites == null || _sprites.Count == 0 || _targetImage == null)
        {
            return;
        }

        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        if (_reverseCts != null)
        {
            _reverseCts.Cancel();
            _reverseCts.Dispose();
        }
        _reverseCts = new CancellationTokenSource();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _reverseCts.Token);
        CancellationToken activeToken = linkedCts.Token;

        for (int i = _sprites.Count - 1; 0 <= i; i--)
        {
            if (_targetImage == null)
            {
                return;
            }
            _targetImage.sprite = _sprites[i];

            if (0 < i)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_frameInterval), ignoreTimeScale: true, cancellationToken: activeToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (_targetImage == null)
                {
                    return;
                }
            }
        }
    }
    #endregion
}
