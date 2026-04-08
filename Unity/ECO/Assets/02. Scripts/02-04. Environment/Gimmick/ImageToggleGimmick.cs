using UnityEngine;

public class ImageToggleGimmick : TerrainGimmickBase
{
    public override void ApplyGimmick(TerrainObject target, bool isActive)
    {
        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = isActive;
        }
    }
}
