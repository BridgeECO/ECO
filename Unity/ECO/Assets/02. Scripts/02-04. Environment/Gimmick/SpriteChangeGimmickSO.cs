using UnityEngine;

[CreateAssetMenu(fileName = "SpriteChangeGimmickSO", menuName = "Environment/Gimmick/SpriteChangeGimmickSO")]
public class SpriteChangeGimmickSO : TerrainGimmickBaseSO
{
    [SerializeField]
    private Sprite _activeSprite;

    [SerializeField]
    private Sprite _inactiveSprite;

    public override TerrainGimmickBase CreateGimmick()
    {
        return new SpriteChangeGimmick(_activeSprite, _inactiveSprite);
    }
}
