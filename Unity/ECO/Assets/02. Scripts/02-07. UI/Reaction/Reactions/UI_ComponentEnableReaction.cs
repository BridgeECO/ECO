using System;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// 지정한 컴포넌트를 켜거나 끈다. 게임오브젝트는 그대로 두고 연출 컴포넌트만 여닫을 때 쓴다.
/// </summary>
[Serializable]
[Preserve]
public class UI_ComponentEnableReaction : UI_InstantReactionBase
{
    [SerializeField]
    private Behaviour _behaviour;

    [SerializeField]
    private bool _isEnabled = true;

    public override EUIReactionChannel Channel => EUIReactionChannel.ComponentEnable;

    // 자리 판정은 대상 오브젝트 단위라, 지정한 컴포넌트가 붙은 오브젝트를 기준으로 삼는다.
    public override GameObject ResolveTarget(UI_ReactionContext context)
    {
        return _behaviour != null ? _behaviour.gameObject : base.ResolveTarget(context);
    }

    protected override void Apply(UI_ReactionContext context, GameObject target)
    {
        if (_behaviour == null)
        {
            return;
        }

        EnsureBaseline(context, target, default, null, _behaviour.enabled);
        _behaviour.enabled = _isEnabled;
    }

    protected override void Revert(UI_ReactionContext context, GameObject target)
    {
        if (_behaviour == null || !TryGetBaseline(context, target, out UI_ReactionBaseline baseline))
        {
            return;
        }

        _behaviour.enabled = baseline.Flag;
    }
}
