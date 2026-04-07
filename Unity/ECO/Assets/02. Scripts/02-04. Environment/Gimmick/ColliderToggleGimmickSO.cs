using UnityEngine;

[CreateAssetMenu(fileName = "ColliderToggleGimmickSO", menuName = "Environment/Gimmick/ColliderToggleGimmickSO")]
public class ColliderToggleGimmickSO : TerrainGimmickBaseSO
{
    public override TerrainGimmickBase CreateGimmick()
    {
        return new ColliderToggleGimmick();
    }
}
