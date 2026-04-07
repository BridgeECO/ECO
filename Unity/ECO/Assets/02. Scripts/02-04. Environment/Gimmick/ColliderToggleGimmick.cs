using UnityEngine;

public class ColliderToggleGimmick : TerrainGimmickBase
{
    public override void ApplyGimmick(TerrainObject target, bool isActive)
    {
        Collider2D terrainCollider = target.GetComponent<Collider2D>();
        if (terrainCollider != null)
        {
            terrainCollider.enabled = isActive;
        }
    }
}
