using UnityEngine;

public class SpriteChangeGimmick : TerrainGimmickBase
{
    private Sprite _activeSprite;
    private Sprite _inactiveSprite;

    public SpriteChangeGimmick(
        EGimmickActivationType activationType,
        bool isInverted,
        Sprite activeSprite,
        Sprite inactiveSprite)
        : base(activationType, isInverted)
    {
        _activeSprite = activeSprite;
        _inactiveSprite = inactiveSprite;
    }

    protected override void ApplyGimmick(TerrainObject target, bool isActivated)
    {
        SpriteRenderer targetSpriteRenderer = target.GetComponent<SpriteRenderer>();
        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.sprite = isActivated ? _activeSprite : _inactiveSprite;
        }
    }
}
