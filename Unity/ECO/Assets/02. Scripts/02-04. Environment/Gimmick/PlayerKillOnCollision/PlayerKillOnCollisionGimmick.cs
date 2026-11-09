using UnityEngine;

public class PlayerKillOnCollisionGimmick : TerrainGimmickBase
{
    // 트리거 콜백은 들어온 상대만 알려 주므로, 피격 이펙트 위치를 잡을 이 지형의 콜라이더를 기억해 둔다.
    private Collider2D _terrainCollider;

    public PlayerKillOnCollisionGimmick(EGimmickActivationType activationType, bool isInverted)
        : base(activationType, isInverted)
    {
    }

    protected override void ApplyGimmick(TerrainObject target, bool isActivated)
    {
        _terrainCollider = target.GetComponent<Collider2D>();
        if (_terrainCollider != null)
        {
            _terrainCollider.isTrigger = isActivated;
        }
    }

    public override void OnTerrainTriggerEnter2D(Collider2D other)
    {
        if (!IsActivated)
        {
            return;
        }

        // 몸통(Player 태그)보다 발·미끄러짐 콜라이더가 먼저 닿아도 그 스텝에 죽도록 태그 대신 붙은 강체로 플레이어를 찾는다.
        Rigidbody2D attachedRigidbody = other.attachedRigidbody;
        if (attachedRigidbody == null || !attachedRigidbody.TryGetComponent(out PlayerLife playerLife))
        {
            return;
        }
        playerLife.InstantKill(EPlayerDeathCause.KillGimmick, other, _terrainCollider);
    }
}
