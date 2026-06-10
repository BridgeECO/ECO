using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
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
    private int _frameDelay = 4;

    [SerializeField]
    private bool _isLoop = true;

    [SerializeField, Range(2f, 10f)]
    private float _loopInterval = 2f;

    private CancellationTokenSource _cts;

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
    }
    #endregion

    #region Logic & Visuals
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
                    await UniTask.DelayFrame(_frameDelay, PlayerLoopTiming.Update, cancellationToken: cancellationToken);
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
            if (remainingDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(remainingDelay), cancellationToken: cancellationToken);
            }
            else
            {
                // _loopInterval이 경과했거나 0 이하인 경우 최소 프레임 딜레이 대기하여 오버헤드 방지
                await UniTask.DelayFrame(_frameDelay, PlayerLoopTiming.Update, cancellationToken: cancellationToken);
            }
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
    #endregion
}
