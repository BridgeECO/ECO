using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 같은 효과음이 아주 짧은 간격으로 겹쳐 울리는 것을 막는다.
///
/// 리액션마다 시각을 들면 소용이 없다. 마우스를 목록 위로 쓸면 한 프레임에 서로 다른
/// 버튼 여러 개가 각자 진입 소리를 내므로, 판정은 클립 단위로 전역이어야 한다.
/// 일시정지 중에도 UI는 살아 있으므로 unscaled 시간을 쓴다.
/// </summary>
public static class UI_ReactionSfxThrottle
{
    private static readonly Dictionary<int, float> _lastPlayedTimes = new Dictionary<int, float>();

    public static bool TryConsume(ESfxClip clip, float minInterval)
    {
        int key = (int)clip;
        float now = Time.unscaledTime;

        if (_lastPlayedTimes.TryGetValue(key, out float lastPlayedTime)
            && now - lastPlayedTime < minInterval)
        {
            return false;
        }

        _lastPlayedTimes[key] = now;
        return true;
    }

    // Enter Play Mode Options로 도메인 리로드를 끄면 static 상태가 플레이 세션을 넘어 남는다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        _lastPlayedTimes.Clear();
    }
}
