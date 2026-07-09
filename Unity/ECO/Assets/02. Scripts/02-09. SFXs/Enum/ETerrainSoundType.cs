/// <summary>
/// 지형 표면 재질에 따라 선택할 Walk/Land/Dash/HitWall SFX 세트를 식별하는 Enum.
/// PlayerSoundHandler가 PlayerSensor 정보와 조합하여 올바른 클립을 선택한다.
/// </summary>
public enum ETerrainSoundType
{
    Scrap,
    Mold,
    Platform,
}
