using UnityEngine;

[CreateAssetMenu(fileName = "GimmickSO_ColliderToggle", menuName = "Scriptable Objects/Terrain Gimmick/ColliderToggleGimmickSO")]
public class ColliderToggleGimmickSO : TerrainGimmickBaseSO
{
    public override TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry)
    {
        return new ColliderToggleGimmick(ActivationType, IsInverted);
    }
}
