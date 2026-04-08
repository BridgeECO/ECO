using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

public class VisualEffectObject : SpecialObjectBase
{
    [Foldout("Project")]
    [SerializeField]
    private EVisualEffectPlayType _visualEffectPlayType;

    [SerializeField]
    private List<VFX> _vfxs;

    private Animator _animator;
    private readonly string _animationTrigger = "Play";

    private bool _hasPlayed = false;

    protected override void Awake()
    {
        base.Awake();
        InitVisualEffectObject();
    }

    private void InitVisualEffectObject()
    {
        switch (_visualEffectPlayType)
        {
            case EVisualEffectPlayType.VFX:
                _vfxs = GetComponentsInChildren<VFX>().ToList();
                break;
            case EVisualEffectPlayType.Animation:
                _animator = GetComponent<Animator>();
                break;
        }
    }

    protected override void Interact()
    {
        base.Interact();
        if (_hasPlayed)
        {
            return;
        }

        switch (_visualEffectPlayType)
        {
            case EVisualEffectPlayType.VFX:
                PlayVFX();
                break;
            case EVisualEffectPlayType.Animation:
                if (_animator != null)
                {
                    PlayAnimationAsync().Forget();
                }
                break;
        }
        _hasPlayed = true;
    }

    private void PlayVFX()
    {
        foreach (var vfx in _vfxs)
        {
            vfx.Play();
        }
    }

    private async UniTaskVoid PlayAnimationAsync()
    {
        _animator.SetTrigger(_animationTrigger);
        await UniTask.Yield();
    }
}
