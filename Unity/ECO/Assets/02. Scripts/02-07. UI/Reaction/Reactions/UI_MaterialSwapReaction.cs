using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

/// <summary>
/// 대상 Graphic의 머티리얼을 미리 만들어 둔 다른 에셋으로 바꾼다.
/// 속성값을 굴리지 않는 이유는 uGUI에 MaterialPropertyBlock이 없어 사본과 Destroy가 필요해서다.
/// </summary>
[Serializable]
[Preserve]
public class UI_MaterialSwapReaction : UI_InstantReactionBase
{
    [SerializeField]
    private Material _material;

    public override EUIReactionChannel Channel => EUIReactionChannel.Material;

    protected override void Apply(UI_ReactionContext context, GameObject target)
    {
        Graphic graphic = target.GetComponent<Graphic>();
        if (graphic == null)
        {
            return;
        }

        EnsureBaseline(context, target, default, graphic.material, false);
        graphic.material = _material;
    }

    protected override void Revert(UI_ReactionContext context, GameObject target)
    {
        Graphic graphic = target.GetComponent<Graphic>();
        if (graphic == null || !TryGetBaseline(context, target, out UI_ReactionBaseline baseline))
        {
            return;
        }

        graphic.material = baseline.Reference as Material;
    }
}
