using UnityEngine;

public class ColliderToggleGimmick : TerrainGimmickBase
{
    public ColliderToggleGimmick(EGimmickActivationType activationType, bool isInverted)
        : base(activationType, isInverted)
    {
    }

    protected override void ApplyGimmick(TerrainObject target, bool isActivated)
    {
        Collider2D terrainCollider = target.GetComponent<Collider2D>();
        if (terrainCollider != null)
        {
            terrainCollider.enabled = isActivated;
        }
    }
}
