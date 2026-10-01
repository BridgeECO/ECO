using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class BossPolygonCollider : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private PolygonCollider2D _polygonCollider;

    private readonly List<Vector2> _physicsShapePoints = new List<Vector2>();
    private bool _hasPendingRefresh;

    private void LateUpdate()
    {
        if (!_hasPendingRefresh)
        {
            return;
        }
        _hasPendingRefresh = false;
        ApplyCurrentSpriteShape();
    }

    public void RefreshCollider()
    {
        _hasPendingRefresh = true;
    }
    private void ApplyCurrentSpriteShape()
    {
        if (_spriteRenderer == null ||
            _spriteRenderer.sprite == null ||
            _polygonCollider == null)
        {
            return;
        }

        Sprite sprite = _spriteRenderer.sprite;
        int shapeCount = sprite.GetPhysicsShapeCount();

        if (shapeCount == 0)
        {
            return;
        }

        _polygonCollider.pathCount = shapeCount;

        for (int pathIndex = 0; pathIndex < shapeCount; pathIndex++)
        {
            _physicsShapePoints.Clear();
            sprite.GetPhysicsShape(pathIndex, _physicsShapePoints);

            ApplySpriteFlip();
            _polygonCollider.SetPath(pathIndex, _physicsShapePoints);
        }
    }
    private void ApplySpriteFlip()
    {
        bool isFlippedX = _spriteRenderer.flipX;
        bool isFlippedY = _spriteRenderer.flipY;

        if (!isFlippedX && !isFlippedY)
        {
            return;
        }

        for (int i = 0; i < _physicsShapePoints.Count; i++)
        {
            Vector2 point = _physicsShapePoints[i];

            if (isFlippedX)
            {
                point.x = -point.x;
            }

            if (isFlippedY)
            {
                point.y = -point.y;
            }

            _physicsShapePoints[i] = point;
        }
    }
}
