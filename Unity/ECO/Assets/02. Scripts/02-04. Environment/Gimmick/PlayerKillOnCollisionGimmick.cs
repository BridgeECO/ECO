using UnityEngine;

public class PlayerKillOnCollisionGimmick : TerrainGimmickBase
{
    public PlayerKillOnCollisionGimmick(EGimmickActivationType activationType, bool isInverted)
        : base(activationType, isInverted)
    {
    }

    protected override void ApplyGimmick(TerrainObject target, bool isActivated)
    {

    }
}
