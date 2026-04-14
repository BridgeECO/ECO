using UnityEngine;

public class ImageToggleGimmick : TerrainGimmickBase
{
    public ImageToggleGimmick(EGimmickActivationType activationType, bool isInverted)
        : base(activationType, isInverted)
    {
    }

    protected override void ApplyGimmick(TerrainObject target, bool isActivated)
    {
        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = isActivated;
        }
    }
}
