using UnityEngine;

public class SpriteChangeGimmick : TerrainGimmickBase
{
    private Sprite _activeSprite;
    private Sprite _inactiveSprite;

    public SpriteChangeGimmick(Sprite activeSprite, Sprite inactiveSprite)
    {
        _activeSprite = activeSprite;
        _inactiveSprite = inactiveSprite;
    }

    public override void ApplyGimmick(TerrainObject target, bool isActive)
    {
        SpriteRenderer targetSpriteRenderer = target.GetComponent<SpriteRenderer>();
        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.sprite = isActive ? _activeSprite : _inactiveSprite;
        }
    }
}
