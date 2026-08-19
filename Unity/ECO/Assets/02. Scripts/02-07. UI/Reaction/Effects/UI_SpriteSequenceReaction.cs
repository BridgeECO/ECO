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
    [Tooltip("반복 주기(초). 쉬는 시간이 아니라 회차가 시작한 시점부터 재는 값이라 재생 시간이 포함됩니다. " +
        "재생이 이 값보다 길면 곧바로 다음 회차를 시작합니다.")]
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
        // 무한 반복은 끝나지 않으므로 "끝날 때까지 무시"가 영구 무시가 된다. 그 경우 정책을 적용하지 않는다.
        if (!_isLoop && Runner.IsPlaying
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

        // WaitUntil 대신 직접 돌린다. hover가 끊길 때마다 지나가는 경로라 람다가 그대로 쓰레기가 된다.
        // 무한 반복은 끝나지 않으므로 여기서 기다리면 아래 Kill에 영영 닿지 못한다. PlayAsync와 같은 예외를 둔다.
        while (!_isLoop && exitPolicy == EUIReactionExitPolicy.PlayToEnd && Runner.IsPlaying)
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
