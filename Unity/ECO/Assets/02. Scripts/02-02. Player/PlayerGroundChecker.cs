using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundChecker
{
    // 지형 모서리가 둥글어서 생기는 V자 홈이나 지형과 발판 이음매의 미세한 단차에서도
    // 접지가 끊기지 않도록, 발 아래로 이만큼 늘린 영역까지 지면으로 본다.
    private const float GROUND_CHECK_DEPTH = 0.1f;

    // 발판 위에 서 있으면 물리 엔진의 접촉 여유만큼 발바닥이 발판 선보다 살짝 내려갈 수 있다.
    private const float PLATFORM_SINK_TOLERANCE = 0.02f;

    private Collider2D _feetCollider;
    private LayerMask _platformLayer;
    private ContactFilter2D _groundFilter;
    private List<Collider2D> _overlapResults = new List<Collider2D>();

    public PlayerGroundChecker(Collider2D feetCollider, LayerMask terrainLayer, LayerMask platformLayer)
    {
        _feetCollider = feetCollider;
        _platformLayer = platformLayer;
        _groundFilter.SetLayerMask(terrainLayer | platformLayer);
    }

    // 발 좌우 절반 아래에 모두 지면이 있어야 선 것으로 본다. 한쪽만 걸쳤다면 무게중심이 모서리 밖으로 넘어간 것이니
    // 접지를 놓아 공중 상태의 미끄러짐이 밀어내게 한다. 발이 실제로 닿았는지는 둥근 모서리에 발끝만 걸려도 참이라
    // 가장자리 판단에 쓸 수 없다.
    public bool CheckOnGround()
    {
        Bounds feet = _feetCollider.bounds;
        return CheckGroundBelow(feet.min.x, feet.center.x, feet) && CheckGroundBelow(feet.center.x, feet.max.x, feet);
    }

    private bool CheckGroundBelow(float minX, float maxX, Bounds feet)
    {
        Vector2 center = new Vector2((minX + maxX) * 0.5f, feet.center.y - GROUND_CHECK_DEPTH * 0.5f);
        Vector2 size = new Vector2(maxX - minX, feet.size.y + GROUND_CHECK_DEPTH);

        int count = Physics2D.OverlapBox(center, size, 0f, _groundFilter, _overlapResults);
        for (int i = 0; i < count; i++)
        {
            if (IsGround(_overlapResults[i], feet.min.y))
            {
                return true;
            }
        }
        return false;
    }

    // 일방향 발판은 아래에서 올라오는 발을 통과시키므로, 발바닥 아래에 있을 때만 딛고 선 것으로 본다.
    private bool IsGround(Collider2D hit, float feetBottomY)
    {
        if (((1 << hit.gameObject.layer) & _platformLayer) == 0)
        {
            return true;
        }
        return hit.bounds.max.y <= feetBottomY + PLATFORM_SINK_TOLERANCE && !PlayerUnderJump.IsPassingThrough(hit);
    }
}
