using UnityEngine;
using VInspector;
using Cysharp.Threading.Tasks;

public class VisualEffectObject : SpecialObjectBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Animator _animator;

    [Foldout("Project")]
    [SerializeField]
    private string _animationTriggerName;

    private bool _hasPlayed = false;

    protected override void Interact()
    {
        base.Interact();

        if (_hasPlayed)
        {
            return;
        }

        if (!ReferenceEquals(_animator, null) && !string.IsNullOrEmpty(_animationTriggerName))
        {
            _hasPlayed = true;
            PlayAnimationAsync().Forget();
        }
    }

    private async UniTaskVoid PlayAnimationAsync()
    {
        _animator.SetTrigger(_animationTriggerName);
        await UniTask.Yield();
    }
}
