using System;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// 대상 게임오브젝트를 켜거나 끈다. 미리 배치해 둔 글로우·테두리 이미지를 여닫는 용도다.
/// 대상이 Reactor 자신이면 OnDisable과 복원이 서로를 부르는 순환이 생겨 무시한다.
/// </summary>
[Serializable]
[Preserve]
public class UI_ActiveReaction : UI_InstantReactionBase
{
    [SerializeField]
    private bool _isActive = true;

    public override EUIReactionChannel Channel => EUIReactionChannel.Active;

    protected override void Apply(UI_ReactionContext context, GameObject target)
    {
        if (target == context.Owner)
        {
            return;
        }

        EnsureBaseline(context, target, default, null, target.activeSelf);
        target.SetActive(_isActive);
    }

    protected override void Revert(UI_ReactionContext context, GameObject target)
    {
        if (target == context.Owner || !TryGetBaseline(context, target, out UI_ReactionBaseline baseline))
        {
            return;
        }

        target.SetActive(baseline.Flag);
    }
}
