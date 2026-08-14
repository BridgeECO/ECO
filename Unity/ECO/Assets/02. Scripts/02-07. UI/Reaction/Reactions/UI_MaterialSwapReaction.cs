using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

/// <summary>
/// 대상 Graphic의 머티리얼을 미리 만들어 둔 다른 에셋으로 바꾼다.
///
/// 속성값을 직접 굴리지 않고 에셋을 통째로 교체하는 이유는, uGUI가
/// MaterialPropertyBlock을 지원하지 않아 속성 변경에는 그래픽마다 머티리얼 사본이 필요하고,
/// 그 사본을 정리하려면 Destroy가 필요하기 때문이다. 아웃라인 on/off나 흑백 처리 같은
/// 실사용 사례는 에셋 두 벌로 충분하다.
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
