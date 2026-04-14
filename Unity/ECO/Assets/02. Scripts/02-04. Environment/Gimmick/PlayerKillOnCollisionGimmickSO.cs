using UnityEngine;

[CreateAssetMenu(fileName = "PlayerKillOnCollisionGimmickSO", menuName = "Scriptable Objects/Terrain Gimmick/PlayerKillOnCollisionGimmickSO")]
public class PlayerKillOnCollisionGimmickSO : TerrainGimmickBaseSO
{
    public override TerrainGimmickBase CreateGimmick()
    {
        return new PlayerKillOnCollisionGimmick(ActivationType, IsInverted);
    }
}
