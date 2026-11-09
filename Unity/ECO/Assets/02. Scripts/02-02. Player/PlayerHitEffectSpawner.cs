using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 피격·사망 순간 플레이어가 닿은 자리에 피격 이펙트를 풀에서 꺼내 놓는다.
/// </summary>
public class PlayerHitEffectSpawner
{
    private const int OVERLAP_CAPACITY = 8;

    private readonly Collider2D _bodyCollider;
    private readonly List<Collider2D> _overlappedColliders = new(OVERLAP_CAPACITY);
    private readonly ContactFilter2D _bossFilter;

    public PlayerHitEffectSpawner(Collider2D bodyCollider)
    {
        _bodyCollider = bodyCollider;
        // 보스 판정 콜라이더는 트리거다. 프로젝트의 queriesHitTriggers 설정에 기대지 않고 필터에 직접 넣는다.
        _bossFilter = new ContactFilter2D { useTriggers = true };
    }

    // 두 콜라이더에서 서로 가장 가까운 두 점의 가운데에 놓는다. 겹쳐 있으면 겹친 깊이의 가운데가 된다.
    public void SpawnAtContact(Collider2D playerCollider, Collider2D other)
    {
        ColliderDistance2D distance = Physics2D.Distance(playerCollider, other);
        if (!distance.isValid)
        {
            return;
        }
        Spawn((distance.pointA + distance.pointB) * 0.5f);
    }

    // 보스는 PlayerLife를 거치지 않고 PlayerDied를 보내 닿은 콜라이더를 알려 주지 않는다.
    // 보스가 몸통(Player 태그)에 들어온 순간에 사망을 보내므로, 그때 몸통과 겹친 보스 콜라이더가 닿은 자리다.
    public void SpawnAtOverlappingBoss()
    {
        int count = _bodyCollider.Overlap(_bossFilter, _overlappedColliders);
        for (int i = 0; i < count; i++)
        {
            Collider2D overlapped = _overlappedColliders[i];
            if (overlapped.CompareTag(nameof(ETags.Boss)))
            {
                SpawnAtContact(_bodyCollider, overlapped);
                return;
            }
        }
    }

    private void Spawn(Vector2 position)
    {
        // Pool.OnGet이 SetActive(true)로 OnEnable의 재생을 먼저 건다. 파티클은 이 프레임 뒤쪽의
        // 파티클 업데이트에서 처음 나오므로, 그 전에 위치를 옮기면 첫 파편도 새 자리에서 나온다.
        GameObject effect = PoolManager.Instance.Pop(EPoolable.PlayerHitEffect);
        if (effect == null)
        {
            return;
        }
        effect.transform.position = position;
    }
}
