using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

/// <summary>
/// 씬·프리팹에 이미 배치된 UI_SpriteAnimator를 구동한다.
///
/// UI_SpriteAnimator는 OnEnable에서 재생을 시작하고 OnDisable에서 취소하므로,
/// 컴포넌트를 켜고 끄는 것이 곧 재생·정지다. 기존 컴포넌트는 손대지 않는다.
///
/// 정지해도 Image는 마지막 프레임에 멈춰 있으므로 원래 스프라이트 복원은 이쪽이 책임진다.
/// </summary>
[Serializable]
[Preserve]
public class UI_SpriteAnimatorReaction : UI_ReactionBase
{
    [SerializeField]
    private UI_SpriteAnimator _animator;

    [SerializeField]
    [Tooltip("애니메이터가 그리는 Image. 비우면 애니메이터와 같은 오브젝트의 Image를 씁니다.")]
    private Image _restoreTarget;

    public override EUIReactionChannel Channel => EUIReactionChannel.Sprite;

    public override bool IsPlaying => _animator != null && _animator.isActiveAndEnabled;

    public override GameObject ResolveTarget(UI_ReactionContext context)
    {
        return _animator != null ? _animator.gameObject : base.ResolveTarget(context);
    }

    public override UniTask PlayAsync(UI_ReactionContext context,
        EUIReactionInterruptPolicy interruptPolicy, CancellationToken cancellationToken)
    {
        if (_animator == null)
        {
            return UniTask.CompletedTask;
        }

        CaptureSpriteBaseline(context);
        _animator.enabled = true;
        return UniTask.CompletedTask;
    }

    public override async UniTask ExitAsync(UI_ReactionContext context,
        EUIReactionExitPolicy exitPolicy, CancellationToken cancellationToken)
    {
        if (exitPolicy == EUIReactionExitPolicy.Keep || _animator == null)
        {
            return;
        }

        // UI_Popup이 닫힐 때 쓰는 것과 같은 역재생 경로다.
        if (exitPolicy == EUIReactionExitPolicy.Reverse && _animator.isActiveAndEnabled)
        {
            await _animator.PlayReverseAsync(cancellationToken).SuppressCancellationThrow();
        }

        RestoreBaseline(context);
    }

    public override void Kill()
    {
        if (_animator != null)
        {
            _animator.enabled = false;
        }
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

    private void CaptureSpriteBaseline(UI_ReactionContext context)
    {
        Image image = ResolveImage(context);
        if (image == null)
        {
            return;
        }

        EnsureBaseline(context, image.gameObject, default, image.sprite, false);
    }

    private Image ResolveImage(UI_ReactionContext context)
    {
        if (_restoreTarget != null)
        {
            return _restoreTarget;
        }

        return _animator == null ? null : _animator.GetComponent<Image>();
    }
}
