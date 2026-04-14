using UnityEngine;

public class PlayerKillOnCollisionGimmick : TerrainGimmickBase
{
    public PlayerKillOnCollisionGimmick(EGimmickActivationType activationType, bool isInverted)
        : base(activationType, isInverted)
    {
    }

    protected override void ApplyGimmick(TerrainObject target, bool isActivated)
    {
        Collider2D terrainCollider = target.GetComponent<Collider2D>();
        if (terrainCollider != null)
        {
            terrainCollider.isTrigger = isActivated;
        }
    }

    public override void OnTerrainTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(ETags.Player.ToString()))
        {
            Debug.Log($"부딪힌 대상이 플레이어가 아님 : {other.tag}");
            return;
        }
        RespawnManager.Instance.Respawn();
    }
}
