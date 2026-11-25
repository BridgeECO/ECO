using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 탑승자가 벨트의 어느 면에 닿아 있는지 가리고, 그 면이 벨트 회전 방향으로 움직이는 쪽을 추가 속도 보정 방향으로 바꾼다.
/// 윗면은 발로 보며, 발이 실제로 닿은 면과 플레이어 접지 판정이 지면으로 보는 발 아래 영역을 함께 본다.
/// 옆면은 몸통으로 보고 위, 아래 중 하나를 고른다. 매 물리 스텝 불리므로 할당하지 않는다.
/// </summary>
public class ConveyorBeltSurfaceResolver
{
    // 플레이어 접지 규칙과 같은 각이다. 이보다 가파른 면(둥근 모서리의 옆쪽 비탈)에 발끝만 걸려 있으면 밀지 않는다.
    private const float MAX_SUPPORT_SLOPE_ANGLE = 45f;

    // 플레이어 접지 판정(PlayerGroundChecker)이 발 가운데 절반 아래로 더 보는 깊이와 같다.
    private const float GROUND_CHECK_DEPTH = 0.1f;

    private EConveyorBeltDirection _direction;
    private ContactFilter2D _topSurfaceFilter;
    private ContactFilter2D _rightSideFilter;
    private ContactFilter2D _leftSideFilter;
    private ContactFilter2D _solidFilter;
    private List<Collider2D> _overlapResults = new List<Collider2D>();

    public ConveyorBeltSurfaceResolver(EConveyorBeltDirection direction)
    {
        _direction = direction;
        _topSurfaceFilter.SetNormalAngle(90f - MAX_SUPPORT_SLOPE_ANGLE, 90f + MAX_SUPPORT_SLOPE_ANGLE);

        // 옆면은 윗면이 아닌 나머지 가로 쪽이다(오른쪽 옆면 법선 -45~45도, 왼쪽 옆면 135~225도). 경계 각 45, 135도는 윗면 필터와
        // 겹치지만, 감지가 윗면을 먼저 보므로 둥근 모서리에서 둘 다 참이어도 윗면이 이긴다.
        _rightSideFilter.SetNormalAngle(MAX_SUPPORT_SLOPE_ANGLE, 360f - MAX_SUPPORT_SLOPE_ANGLE);
        _rightSideFilter.useOutsideNormalAngle = true;
        _leftSideFilter.SetNormalAngle(180f - MAX_SUPPORT_SLOPE_ANGLE, 180f + MAX_SUPPORT_SLOPE_ANGLE);

        // 프로젝트 설정에서 쿼리가 트리거도 잡으므로(queriesHitTriggers) 압력판 같은 트리거를 뺀다.
        _solidFilter.useTriggers = false;
    }

    public bool TryResolveCorrection(Collider2D riderFeet, Collider2D belt, out ESpeedCorrectionDirection direction)
    {
        direction = ESpeedCorrectionDirection.Right;
        if (!riderFeet.IsTouching(belt, _topSurfaceFilter))
        {
            return false;
        }

        direction = ResolveTopSurfaceDirection();
        return true;
    }

    // 윗면의 바깥 법선은 위쪽이다.
    public ESpeedCorrectionDirection ResolveTopSurfaceDirection()
    {
        Vector2 tangent = GetSurfaceTangent(Vector2.up);
        return 0f < tangent.x ? ESpeedCorrectionDirection.Right : ESpeedCorrectionDirection.Left;
    }

    // 옆면은 세로로만 민다. 비스듬한 옆면도 바깥 법선을 수평(오른쪽 옆면 = 오른쪽, 왼쪽 옆면 = 왼쪽)으로 보고 위, 아래 중 하나를 고른다.
    public bool TryResolveSideCorrection(Collider2D riderBody, Collider2D belt, out ESpeedCorrectionDirection direction)
    {
        direction = ESpeedCorrectionDirection.Up;
        Vector2 outwardNormal;
        if (riderBody.IsTouching(belt, _rightSideFilter))
        {
            outwardNormal = Vector2.right;
        }
        else if (riderBody.IsTouching(belt, _leftSideFilter))
        {
            outwardNormal = Vector2.left;
        }
        else
        {
            return false;
        }

        Vector2 tangent = GetSurfaceTangent(outwardNormal);
        direction = 0f < tangent.y ? ESpeedCorrectionDirection.Up : ESpeedCorrectionDirection.Down;
        return true;
    }

    // 벨트 윗면에 닿아 있지 않은 발 아래를 플레이어 접지 판정과 같은 영역(발 가운데 절반 폭, 발 아래 깊이 0.1)으로 본다.
    // 지상 상태는 y 속도가 0이라 0.1보다 얕게 낮은 지면 위에서는 발이 떠 있으므로, 실제 접촉만 보면 그 지면을 놓친다.
    // 받치는 면에 실제로 닿았거나 영역 안에 이 벨트가 아닌 지면이 있으면 다른 지면 위다.
    public void ScanGroundBelow(Collider2D riderFeet, Collider2D belt, out bool isBeltBelow, out bool isOnOtherSupport)
    {
        isBeltBelow = false;
        isOnOtherSupport = riderFeet.IsTouching(_topSurfaceFilter);

        Bounds feet = riderFeet.bounds;
        Vector2 center = new Vector2(feet.center.x, feet.center.y - GROUND_CHECK_DEPTH * 0.5f);
        Vector2 size = new Vector2(feet.extents.x, feet.size.y + GROUND_CHECK_DEPTH);

        int count = Physics2D.OverlapBox(center, size, 0f, _solidFilter, _overlapResults);
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = _overlapResults[i];
            if (hit == belt)
            {
                isBeltBelow = true;
            }
            else if (hit.attachedRigidbody != riderFeet.attachedRigidbody)
            {
                isOnOtherSupport = true;
            }
        }
    }

    // 벨트 윗면 바로 위(접지 판정 깊이만큼)에 들어온 플레이어 발을 찾는다. 발이 떠 있어 충돌 콜백이 오지 않는 탑승자를 잡는다.
    // Box 콜라이더의 bounds는 둥근 모서리 반지름을 빼고 잡히므로, 윗면 높이는 실제 모양에서 구한다.
    public bool TryFindFeetAbove(Collider2D belt, out Collider2D riderFeet)
    {
        riderFeet = null;
        Bounds bounds = belt.bounds;
        float topY = belt.ClosestPoint(new Vector2(bounds.center.x, bounds.max.y + 1f)).y;
        Vector2 center = new Vector2(bounds.center.x, topY + GROUND_CHECK_DEPTH * 0.5f);
        Vector2 size = new Vector2(bounds.size.x, GROUND_CHECK_DEPTH);

        int count = Physics2D.OverlapBox(center, size, 0f, _solidFilter, _overlapResults);
        for (int i = 0; i < count; i++)
        {
            if (_overlapResults[i].CompareTag(nameof(ETags.PlayerFeet)))
            {
                riderFeet = _overlapResults[i];
                return true;
            }
        }
        return false;
    }

    // 시계방향으로 도는 벨트의 면은 바깥 법선 n을 시계방향으로 90도 돌린 접선 (n.y, -n.x) 쪽으로 움직인다.
    // 윗면(n = 위)은 오른쪽, 오른쪽 옆면(n = 오른쪽)은 아래쪽, 왼쪽 옆면(n = 왼쪽)은 위쪽이다. 반시계방향은 그 반대다.
    private Vector2 GetSurfaceTangent(Vector2 outwardNormal)
    {
        Vector2 clockwiseTangent = new Vector2(outwardNormal.y, -outwardNormal.x);
        return _direction == EConveyorBeltDirection.Clockwise ? clockwiseTangent : -clockwiseTangent;
    }
}
