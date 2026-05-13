using UnityEngine;

[CreateAssetMenu(fileName = "GimmickSO_PlayerKillOnCollision", menuName = "Scriptable Objects/Terrain Gimmick/PlayerKillOnCollisionGimmickSO")]
public class PlayerKillOnCollisionGimmickSO : TerrainGimmickBaseSO
{
    public override TerrainGimmickBase CreateGimmick(TerrainGimmickEntry entry)
    {
        return new PlayerKillOnCollisionGimmick(ActivationType, IsInverted);
    }
}
