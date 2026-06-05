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

    [SerializeField]
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

        int currentIndex = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            _targetImage.sprite = _sprites[currentIndex];

            if (currentIndex == _sprites.Count - 1)
            {
                OnAnimationComplete?.Invoke();

                if (!_isLoop)
                {
                    break;
                }

                try
                {
                    if (_loopInterval > 0f)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(_loopInterval), cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await UniTask.DelayFrame(_frameDelay, PlayerLoopTiming.Update, cancellationToken: cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                currentIndex = 0;
            }
            else
            {
                try
                {
                    await UniTask.DelayFrame(_frameDelay, PlayerLoopTiming.Update, cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                currentIndex++;
            }
        }
    }
    #endregion
}
