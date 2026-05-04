using UnityEngine;

[CreateAssetMenu(fileName = "GimmickSO_ImageChange", menuName = "Scriptable Objects/Terrain Gimmick/ImageChangeGimmickSO")]
public class ImageChangeGimmickSO : TerrainGimmickBaseSO
{
    public override TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry)
    {
        return new ImageChangeGimmick(ActivationType, IsInverted, entry.ChangeSprite);
    }
}
