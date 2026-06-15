using UnityEngine;

public static class BossPhysicsUtility
{
    public static Vector2 GetGeometricParabola(Vector2 start, Vector2 end, float height, float t)
    {
        Vector2 mid = Vector2.Lerp(start, end, t);

        float arc = 4f * height * t * (1f - t);

        return new Vector2(mid.x, mid.y + arc);
    }

    public static float ApproximateParabolaLength(Vector2 start, Vector2 end, float height, int samples = 10)
    {
        float length = 0f;
        Vector2 lastPoint = start;

        for (int i = 1; i <= samples; i++)
        {
            float t = (float)i / samples;
            Vector2 nextPoint = GetGeometricParabola(start, end, height, t);
            length += Vector2.Distance(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
        return length;
    }
}
