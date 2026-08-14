using System;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>
/// 대상 게임오브젝트를 켜거나 끈다. 미리 배치해 둔 글로우·테두리 이미지를 여닫는 용도다.
///
/// 대상으로 Reactor 자신을 지정하면 끄는 순간 OnDisable이 돌며 복원이 다시 켜는 순환이
/// 생기므로 무시한다. 자기 자신을 감추려면 Alpha 리액션을 쓴다.
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
