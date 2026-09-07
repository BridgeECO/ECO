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

    public override void ResetState()
    {
        base.ResetState();
        _hasPlayed = false;

        switch (_visualEffectPlayType)
        {
            case EVisualEffectPlayType.VFX:
                StopVFX();
                break;
            case EVisualEffectPlayType.Animation:
                if (_animator != null)
                {
                    _animator.Rebind();
                    _animator.Update(0f);
                }
                break;
        }
    }

    private void StopVFX()
    {
        if (_vfxs == null) return;
        foreach (var vfx in _vfxs)
        {
            vfx.Stop();
        }
    }
}
