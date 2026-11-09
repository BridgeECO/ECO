using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundChecker
{
    // 지형 모서리가 둥글어서 생기는 V자 홈이나 지형과 발판 이음매의 미세한 단차에서도
    // 접지가 끊기지 않도록, 발 가운데 아래로 이만큼 늘린 영역까지 지면으로 본다.
    private const float GROUND_CHECK_DEPTH = 0.1f;

    // 발판 위에 서 있으면 물리 엔진의 접촉 여유만큼 발바닥이 발판 선보다 살짝 내려갈 수 있다.
    private const float PLATFORM_SINK_TOLERANCE = 0.02f;

    // 이보다 가파른 지형 면은 발을 받치지 못한다고 본다. 둥근 모서리(반지름 0.2)의 옆쪽 비탈에
    // 발끝만 걸린 채 지상 상태로 굳지 않게 한다.
    private const float MAX_SUPPORT_SLOPE_ANGLE = 45f;

    private Collider2D _feetCollider;
    private LayerMask _platformLayer;
    private ContactFilter2D _groundFilter;
    private ContactFilter2D _platformFilter;
    private ContactFilter2D _terrainSupportFilter;
    private List<Collider2D> _overlapResults = new List<Collider2D>();

    public PlayerGroundChecker(Collider2D feetCollider, LayerMask terrainLayer, LayerMask platformLayer)
    {
        _feetCollider = feetCollider;
        _platformLayer = platformLayer;
        _groundFilter.SetLayerMask(terrainLayer | platformLayer);
        _platformFilter.SetLayerMask(platformLayer);
        _terrainSupportFilter.SetLayerMask(terrainLayer);
        _terrainSupportFilter.SetNormalAngle(90f - MAX_SUPPORT_SLOPE_ANGLE, 90f + MAX_SUPPORT_SLOPE_ANGLE);
    }

    // 기획서 26판은 발이 작고 망토가 길어 플레이어가 아래쪽 판정을 길게 여긴다고 보고 발을 넓혔다. 그래서 넓힌 발 어디든
    // 지면에 얹혀 있으면 선 것으로 보고, 발이 모두 벗어나야 공중 상태로 넘겨 미끄러짐 선이 모서리에서 밀어내게 한다.
    // 발 전체 폭은 실제로 받치는 면만 본다. 아래로 늘린 영역으로 보면 둥근 모서리 위에 발이 뜬 채로 굳는다(BUG-003).
    public bool CheckOnGround()
    {
        if (_feetCollider.IsTouching(_terrainSupportFilter))
        {
            return true;
        }

        Bounds feet = _feetCollider.bounds;
        if (CheckGroundBelow(feet.min.x, feet.max.x, 0f, _platformFilter, feet))
        {
            return true;
        }

        float centerHalfExtent = feet.extents.x * 0.5f;
        return CheckGroundBelow(feet.center.x - centerHalfExtent, feet.center.x + centerHalfExtent,
            GROUND_CHECK_DEPTH, _groundFilter, feet);
    }

    private bool CheckGroundBelow(float minX, float maxX, float depth, ContactFilter2D filter, Bounds feet)
    {
        Vector2 center = new Vector2((minX + maxX) * 0.5f, feet.center.y - depth * 0.5f);
        Vector2 size = new Vector2(maxX - minX, feet.size.y + depth);

        int count = Physics2D.OverlapBox(center, size, 0f, filter, _overlapResults);
        for (int i = 0; i < count; i++)
        {
            if (IsGround(_overlapResults[i], feet.min.y))
            {
                return true;
            }
        }
        return false;
    }

    // 발 높이 안에 걸린 일방향 발판 위로 발바닥을 올리는 데 필요한 높이를 구한다.
    // 발바닥에 살짝 파묻힌 정도는 이미 서 있는 것이라 제외한다.
    public bool TryGetPlatformLiftHeight(out float liftHeight)
    {
        Bounds feet = _feetCollider.bounds;
        float minY = feet.min.y + PLATFORM_SINK_TOLERANCE;
        float maxY = feet.max.y + PLATFORM_SINK_TOLERANCE;
        Vector2 center = new Vector2(feet.center.x, (minY + maxY) * 0.5f);
        Vector2 size = new Vector2(feet.size.x, maxY - minY);

        float platformTopY = minY;
        int count = Physics2D.OverlapBox(center, size, 0f, _platformFilter, _overlapResults);
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = _overlapResults[i];
            float hitTopY = hit.bounds.max.y;
            if (hitTopY <= maxY && !PlayerUnderJump.IsPassingThrough(hit))
            {
                platformTopY = Mathf.Max(platformTopY, hitTopY);
            }
        }

        liftHeight = platformTopY - feet.min.y;
        return minY < platformTopY;
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
