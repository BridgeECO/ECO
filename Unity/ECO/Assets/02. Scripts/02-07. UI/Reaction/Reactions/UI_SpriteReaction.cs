using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

/// <summary>대상 Image의 스프라이트를 갈아 끼운다.</summary>
[Serializable]
[Preserve]
public class UI_SpriteReaction : UI_InstantReactionBase
{
    [SerializeField]
    private Sprite _sprite;

    public override EUIReactionChannel Channel => EUIReactionChannel.Sprite;

    protected override void Apply(UI_ReactionContext context, GameObject target)
    {
        Image image = target.GetComponent<Image>();
        if (image == null || _sprite == null)
        {
            return;
        }

        EnsureBaseline(context, target, default, image.sprite, false);
        image.sprite = _sprite;
    }

    protected override void Revert(UI_ReactionContext context, GameObject target)
    {
        Image image = target.GetComponent<Image>();
        if (image == null || !TryGetBaseline(context, target, out UI_ReactionBaseline baseline))
        {
            return;
        }

        image.sprite = baseline.Reference as Sprite;
    }
}
