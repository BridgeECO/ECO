using System.Collections.Generic;
using UnityEngine;

public static class BossPathSectionMigration
{
    private const float DEFAULT_SPEED_MULTIPLIER = 1f;

    public static void Populate(
        List<BossPathSection> sections,
        IReadOnlyList<Transform> waypoints,
        Transform endPoint,
        IReadOnlyList<float> speedMultipliers)
    {
        if (sections.Count != 0)
        {
            return;
        }

        for (int i = 0; i < waypoints.Count; i++)
        {
            AddSection(sections, waypoints[i], GetSpeedMultiplier(speedMultipliers, i));
        }

        AddSection(sections, endPoint, GetSpeedMultiplier(speedMultipliers, waypoints.Count));
    }

    private static void AddSection(List<BossPathSection> sections, Transform endPoint, float speedMultiplier)
    {
        if (endPoint == null)
        {
            return;
        }

        sections.Add(new BossPathSection(endPoint, speedMultiplier));
    }

    private static float GetSpeedMultiplier(IReadOnlyList<float> speedMultipliers, int sectionIndex)
    {
        if (speedMultipliers is null || speedMultipliers.Count <= sectionIndex)
        {
            return DEFAULT_SPEED_MULTIPLIER;
        }

        float speedMultiplier = speedMultipliers[sectionIndex];
        return 0f < speedMultiplier ? speedMultiplier : DEFAULT_SPEED_MULTIPLIER;
    }
}
