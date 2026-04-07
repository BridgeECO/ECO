using UnityEngine;

[CreateAssetMenu(fileName = "ColliderToggleGimmickSO", menuName = "Scriptable Objects/Terrain Gimmick/ColliderToggleGimmickSO")]
public class ColliderToggleGimmickSO : TerrainGimmickBaseSO
{
    public override TerrainGimmickBase CreateGimmick()
    {
        return new ColliderToggleGimmick();
    }
}
