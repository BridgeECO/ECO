public enum EEventType
{
    RoomChanged,
    PlayerDied,
    LifeChanged,
    PlayerRespawned,
    RespawnReset,
    SavePointUpdated,
    RegionInitialized,
    SavePointReached,
    RespawnStateRestored,
    SceneTransitionStarted,
    EnergyLineTrackingRequested,
    TransitionStateChanged,
    GameplaySceneChanged,
    // PlayerDied는 낙사와 장애물 피격 사망을 구분하지 않는다. 낙사만 가려내야 하는 쪽(튜토리얼 조건 등)을 위해
    // 즉사 지형에 닿은 시점 자체를 따로 알린다.
    PlayerInstantKilled,
    MaxCount
}
