using UnityEngine;

[CreateAssetMenu(fileName = "GimmickSO_ImageToggle", menuName = "Scriptable Objects/Terrain Gimmick/ImageToggleGimmickSO")]
public class ImageToggleGimmickSO : TerrainGimmickBaseSO
{
    public override TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry)
    {
        return new ImageToggleGimmick(ActivationType, IsInverted);
    }
}
