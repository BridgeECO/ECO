using UnityEngine;

[CreateAssetMenu(fileName = "ImageToggleGimmickSO", menuName = "Environment/Gimmick/ImageToggleGimmickSO")]
public class ImageToggleGimmickSO : TerrainGimmickBaseSO
{
    public override TerrainGimmickBase CreateGimmick()
    {
        return new ImageToggleGimmick();
    }
}
