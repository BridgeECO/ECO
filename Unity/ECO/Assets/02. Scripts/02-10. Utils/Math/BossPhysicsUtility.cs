using UnityEngine;

public static class BossPhysicsUtility
{
    public static Vector2 CalculateParabolicVelocity(Vector3 start, Vector3 end, float height, float horizontalForce)
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y) * 50f;
        float directionX = Mathf.Sign(end.x - start.x);
        float vx = horizontalForce * directionX;
        float vy = Mathf.Sqrt(2 * gravity * height);
        return new Vector2(vx, vy);
    }
    public static Vector2 GetParabolicPosition(float t, float vx, float vy)
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y) * 50f;
        float x = vx * t;
        float y = (vy * t) - (0.5f * gravity * t * t);
        return new Vector2(x, y);
    }
}
