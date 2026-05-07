using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviourSingleton<BossManager>
{
    private readonly Dictionary<EBoss, BossBase> _bosses = new Dictionary<EBoss, BossBase>();

    public static bool HasInstance => Instance != null;

    public void RegisterBoss(EBoss bossType, BossBase boss)
    {
        if (!_bosses.TryAdd(bossType, boss))
        {
            Debug.LogWarning($"Boss of type '{bossType}' is already registered.");
        }
    }

    public void UnregisterBoss(EBoss bossType)
    {
        _bosses.Remove(bossType);
    }

    public BossBase GetBoss(EBoss bossType)
    {
        if (_bosses.TryGetValue(bossType, out BossBase boss))
        {
            return boss;
        }

        return null;
    }
}