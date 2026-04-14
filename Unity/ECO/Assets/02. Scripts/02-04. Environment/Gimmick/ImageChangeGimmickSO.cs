using UnityEngine;

[CreateAssetMenu(fileName = "ImageChangeGimmickSO", menuName = "Scriptable Objects/Terrain Gimmick/ImageChangeGimmickSO")]
public class ImageChangeGimmickSO : TerrainGimmickBaseSO
{
    [SerializeField]
    private Sprite _activeSprite;

    [SerializeField]
    private Sprite _inactiveSprite;

    public override TerrainGimmickBase CreateGimmick()
    {
        return new ImageChangeGimmick(ActivationType, IsInverted, _activeSprite, _inactiveSprite);
    }
}
