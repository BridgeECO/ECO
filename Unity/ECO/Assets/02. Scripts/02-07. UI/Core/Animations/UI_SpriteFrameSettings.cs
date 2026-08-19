using UnityEngine;

/// <summary>
/// 스프라이트 프레임 재생 설정 한 벌. 호출부마다 같은 인자를 줄줄이 넘기지 않도록 묶어 둔다.
/// </summary>
public readonly struct UI_SpriteFrameSettings
{
    public readonly float FrameInterval;
    public readonly bool IsLoop;
    public readonly float LoopInterval;
    public readonly bool IsIgnoreTimeScale;

    public UI_SpriteFrameSettings(float frameInterval, bool isLoop, float loopInterval, bool isIgnoreTimeScale)
    {
        FrameInterval = frameInterval;
        IsLoop = isLoop;
        LoopInterval = loopInterval;
        IsIgnoreTimeScale = isIgnoreTimeScale;
    }

    /// <summary>반복 간격을 재는 기준 시각. 일시정지 중에도 UI가 돌아야 하면 unscaled를 쓴다.</summary>
    public float GetTime()
    {
        return IsIgnoreTimeScale ? Time.unscaledTime : Time.time;
    }
}
