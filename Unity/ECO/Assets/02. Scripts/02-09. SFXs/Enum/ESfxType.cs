/// <summary>
/// SFX 채널 종류. 채널에 따라 재생 규칙(공간음향 여부, 볼륨 관리 방식)이 달라진다.
/// </summary>
public enum ESfxType
{
    /// <summary>화면 어디서든 중앙(2D) 재생. 동시 재생 수 런타임 조절 가능.</summary>
    Player,

    /// <summary>뷰포트 기준 위치에 따라 볼륨이 감쇠되는 월드 공간 SFX. SoundEmitter 컴포넌트로 제어.</summary>
    World,

    /// <summary>공간음향 없는 2D SFX. UI 조작 피드백용.</summary>
    UI,
}
