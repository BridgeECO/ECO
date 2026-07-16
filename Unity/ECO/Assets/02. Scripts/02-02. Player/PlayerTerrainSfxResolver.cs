/// <summary>
/// 단일 책임 원칙(SRP) 및 클래스 길이 200줄 미만 규칙을 충족하기 위해
/// PlayerSoundHandler에서 분리된 지형 사운드 매핑 도우미 클래스.
/// </summary>
public static class PlayerTerrainSfxResolver
{
    public static ETerrainSoundType ResolveCurrentTerrainType(PlayerSensor sensor, ETerrainSoundType defaultTerrainType)
    {
        if (sensor.IsOnPlatform)
        {
            return ETerrainSoundType.Platform;
        }
        return defaultTerrainType;
    }

    public static ESfxClip ResolveWalkClip(PlayerSensor sensor, ETerrainSoundType defaultTerrainType)
    {
        return ResolveCurrentTerrainType(sensor, defaultTerrainType) switch
        {
            ETerrainSoundType.Scrap    => ESfxClip.SE_PC_WalkingScrap,
            ETerrainSoundType.Mold     => ESfxClip.SE_PC_WalkingMold,
            ETerrainSoundType.Platform => ESfxClip.SE_PC_WalkingPlatform,
            _                          => ESfxClip.SE_PC_WalkingScrap,
        };
    }

    public static ESfxClip ResolveLandClip(PlayerSensor sensor, ETerrainSoundType defaultTerrainType)
    {
        return ResolveCurrentTerrainType(sensor, defaultTerrainType) switch
        {
            ETerrainSoundType.Scrap    => ESfxClip.SE_PC_LandScrap,
            ETerrainSoundType.Mold     => ESfxClip.SE_PC_LandMold,
            ETerrainSoundType.Platform => ESfxClip.SE_PC_LandPlatform,
            _                          => ESfxClip.SE_PC_LandScrap,
        };
    }

    public static ESfxClip ResolveDashClip(PlayerSensor sensor, ETerrainSoundType defaultTerrainType)
    {
        if (!sensor.IsOnGround)
        {
            return ESfxClip.SE_PC_DashAir;
        }

        return ResolveCurrentTerrainType(sensor, defaultTerrainType) switch
        {
            ETerrainSoundType.Scrap    => ESfxClip.SE_PC_DashScrap,
            ETerrainSoundType.Mold     => ESfxClip.SE_PC_DashMold,
            ETerrainSoundType.Platform => ESfxClip.SE_PC_DashPlatform,
            _                          => ESfxClip.SE_PC_DashScrap,
        };
    }

    public static ESfxClip ResolveHitWallClip(PlayerSensor sensor, ETerrainSoundType defaultTerrainType)
    {
        return ResolveCurrentTerrainType(sensor, defaultTerrainType) switch
        {
            ETerrainSoundType.Scrap    => ESfxClip.SE_PC_HitWallScrap,
            ETerrainSoundType.Mold     => ESfxClip.SE_PC_HitWallMold,
            ETerrainSoundType.Platform => ESfxClip.SE_PC_HitWallPlatform,
            _                          => ESfxClip.SE_PC_HitWallScrap,
        };
    }
}
