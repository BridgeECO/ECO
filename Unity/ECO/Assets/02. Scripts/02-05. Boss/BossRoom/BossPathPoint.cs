using UnityEngine;

public readonly struct BossPathPoint
{
    public Vector3 Position { get; }
    public float SpeedMultiplier { get; }

    public BossPathPoint(Vector3 position, float speedMultiplier)
    {
        Position = position;
        SpeedMultiplier = speedMultiplier;
    }
}
