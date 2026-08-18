using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

/// <summary>
/// 스프라이트를 프레임 단위로 갈아 끼워 애니메이션을 만든다.
/// 이 프로젝트는 UI 애니메이션에 Animator를 쓰지 않고 이 방식으로 통일한다.
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
    [Tooltip("반복 사이에 쉬는 시간(초). 0이면 곧바로 다음 회차를 시작합니다.")]
    private float _loopInterval = 0f;

    [SerializeField]
    private bool _isIgnoreTimeScale = true;

    private UI_SpriteFrameRunner _runner;

    public override EUIReactionChannel Channel => EUIReactionChannel.Sprite;

    public override bool IsPlaying => Runner.IsPlaying;

    // [SerializeReference] 역직렬화 경로에서 초기화가 도는지에 기대지 않고 지연 생성한다.
    private UI_SpriteFrameRunner Runner
    {
        get
        {
            if (_runner == null)
            {
                _runner = new UI_SpriteFrameRunner();
            }

            return _runner;
        }
    }

    public override UniTask PlayAsync(UI_ReactionContext context,
        EUIReactionInterruptPolicy interruptPolicy, CancellationToken cancellationToken)
    {
        if (Runner.IsPlaying
            && (interruptPolicy == EUIReactionInterruptPolicy.IgnoreUntilDone
                || interruptPolicy == EUIReactionInterruptPolicy.SkipIfSame))
        {
            return UniTask.CompletedTask;
        }

        Image image = ResolveImage(context);
        if (image == null || _sprites.Count == 0)
        {
            return UniTask.CompletedTask;
        }

        EnsureBaseline(context, image.gameObject, default, image.sprite, false);
        UniTask playTask = Runner.PlayAsync(image, _sprites, BuildSettings(), true, cancellationToken);

        // 무한 반복은 끝나지 않으므로 기다리면 호출부가 영영 풀리지 않는다.
        if (_isLoop)
        {
            playTask.Forget();
            return UniTask.CompletedTask;
        }

        return playTask;
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
        while (exitPolicy == EUIReactionExitPolicy.PlayToEnd && Runner.IsPlaying)
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
                await Runner.PlayAsync(image, _sprites, BuildSettings(), false, cancellationToken);
            }
        }

        RestoreBaseline(context);
    }

    public override void Kill()
    {
        Runner.Stop();
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

    private UI_SpriteFrameSettings BuildSettings()
    {
        return new UI_SpriteFrameSettings(_frameInterval, _isLoop, _loopInterval, _isIgnoreTimeScale);
    }

    private Image ResolveImage(UI_ReactionContext context)
    {
        GameObject target = ResolveTarget(context);
        return target == null ? null : target.GetComponent<Image>();
    }
}
