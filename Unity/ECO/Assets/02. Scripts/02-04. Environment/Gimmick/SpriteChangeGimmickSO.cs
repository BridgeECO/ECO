using UnityEngine;

[CreateAssetMenu(fileName = "SpriteChangeGimmickSO", menuName = "Scriptable Objects/Terrain Gimmick/SpriteChangeGimmickSO")]
public class SpriteChangeGimmickSO : TerrainGimmickBaseSO
{
    [SerializeField]
    private Sprite _activeSprite;

    [SerializeField]
    private Sprite _inactiveSprite;

    public override TerrainGimmickBase CreateGimmick()
    {
        return new SpriteChangeGimmick(ActivationType, IsInverted, _activeSprite, _inactiveSprite);
    }
}
