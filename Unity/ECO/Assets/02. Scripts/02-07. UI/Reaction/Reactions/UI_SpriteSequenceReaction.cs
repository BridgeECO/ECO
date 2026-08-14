using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

/// <summary>
/// 스프라이트를 프레임 단위로 갈아 끼워 애니메이션을 만든다.
/// 이 프로젝트는 최적화를 위해 UI 애니메이션에 Animator를 쓰지 않고 이 방식으로 통일한다.
///
/// 이탈 시 되감기를 고르면 프레임을 역순으로 돌려준다. 요청서의 Reverse on Roll Off가 여기에 해당한다.
/// </summary>
[Serializable]
[Preserve]
public class UI_SpriteSequenceReaction : UI_ReactionBase
{
    [SerializeField]
    private List<Sprite> _sprites = new List<Sprite>();

    [SerializeField]
    [Tooltip("프레임 간격(초). 0.0333이면 약 30프레임입니다.")]
    private float _frameInterval = 0.0333f;

    [SerializeField]
    private bool _isLoop = false;

    [SerializeField]
    private bool _isIgnoreTimeScale = true;

    private CancellationTokenSource _cts;
    private bool _isPlaying;

    public override EUIReactionChannel Channel => EUIReactionChannel.Sprite;

    public override bool IsPlaying => _isPlaying;

    public override async UniTask PlayAsync(UI_ReactionContext context,
        EUIReactionInterruptPolicy interruptPolicy, CancellationToken cancellationToken)
    {
        if (_isPlaying
            && (interruptPolicy == EUIReactionInterruptPolicy.IgnoreUntilDone
                || interruptPolicy == EUIReactionInterruptPolicy.SkipIfSame))
        {
            return;
        }

        Image image = ResolveImage(context);
        if (image == null || _sprites.Count == 0)
        {
            return;
        }

        EnsureBaseline(context, image.gameObject, default, image.sprite, false);
        Kill();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // 무한 반복은 끝나지 않으므로 기다리면 호출부가 영영 풀리지 않는다.
        if (_isLoop)
        {
            PlayFramesAsync(image, true, _cts.Token).Forget();
            return;
        }

        await PlayFramesAsync(image, true, _cts.Token);
    }

    public override async UniTask ExitAsync(UI_ReactionContext context,
        EUIReactionExitPolicy exitPolicy, CancellationToken cancellationToken)
    {
        if (exitPolicy == EUIReactionExitPolicy.Keep)
        {
            return;
        }

        // 람다를 쓰는 WaitUntil 대신 직접 돌린다. hover가 끊길 때마다 지나가는 경로라
        // 여기서 델리게이트를 새로 잡으면 그대로 쓰레기가 된다.
        while (exitPolicy == EUIReactionExitPolicy.PlayToEnd && _isPlaying)
        {
            bool isCanceled = await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken)
                .SuppressCancellationThrow();
            if (isCanceled)
            {
                break;
            }
        }

        Kill();

        if (exitPolicy == EUIReactionExitPolicy.Reverse)
        {
            Image image = ResolveImage(context);
            if (image != null)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                await PlayFramesAsync(image, false, _cts.Token);
            }
        }

        RestoreBaseline(context);
    }

    public override void Kill()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        _isPlaying = false;
    }

    public override void RestoreBaseline(UI_ReactionContext context)
    {
        Kill();

        Image image = ResolveImage(context);
        if (image == null || !TryGetBaseline(context, image.gameObject, out UI_ReactionBaseline baseline))
        {
            return;
        }

        image.sprite = baseline.Reference as Sprite;
    }

    private Image ResolveImage(UI_ReactionContext context)
    {
        GameObject target = ResolveTarget(context);
        return target == null ? null : target.GetComponent<Image>();
    }

    private async UniTask PlayFramesAsync(Image image, bool isForward, CancellationToken token)
    {
        _isPlaying = true;
        DelayType delayType = _isIgnoreTimeScale ? DelayType.UnscaledDeltaTime : DelayType.DeltaTime;

        try
        {
            do
            {
                for (int i = 0; i < _sprites.Count; i++)
                {
                    if (image == null)
                    {
                        return;
                    }

                    image.sprite = _sprites[isForward ? i : _sprites.Count - 1 - i];

                    if (i == _sprites.Count - 1)
                    {
                        break;
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(_frameInterval), delayType,
                        PlayerLoopTiming.Update, token);
                }
            }
            while (_isLoop && isForward && !token.IsCancellationRequested);
        }
        catch (OperationCanceledException)
        {
            // 취소는 이 시스템의 정상 경로다. hover가 끊길 때마다 들어온다.
        }
        finally
        {
            _isPlaying = false;
        }
    }
}
