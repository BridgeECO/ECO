using UnityEngine;

public class ImageChangeGimmick : TerrainGimmickBase
{
    private Sprite _changeSprite;
    private Sprite _originalSprite;

    public ImageChangeGimmick(EGimmickActivationType activationType, bool isInverted, Sprite changeSprite)
        : base(activationType, isInverted)
    {
        _changeSprite = changeSprite;
    }

    protected override void ApplyGimmick(TerrainObject target, bool isActivated)
    {
        SpriteRenderer targetSpriteRenderer = target.GetComponent<SpriteRenderer>();
        if (targetSpriteRenderer == null)
        {
            return;
        }

        if (_originalSprite == null)
        {
            _originalSprite = targetSpriteRenderer.sprite;
        }

        targetSpriteRenderer.sprite = isActivated ? _changeSprite : _originalSprite;
    }
}
