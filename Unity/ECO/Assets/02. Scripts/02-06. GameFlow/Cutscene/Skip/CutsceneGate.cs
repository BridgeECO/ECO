using UnityEngine;

/// <summary>
/// 컷씬이 재생 중인지 전역에 알린다. ESC를 함께 쓰는 쪽(일시정지 메뉴, NPC 대화 취소)이 이걸 보고 비켜난다.
///
/// 이벤트가 아니라 static 읽기인 이유: Update 실행 순서가 정해져 있지 않고, UIManager는
/// EventManager가 아직 없는 로드 순서를 겪어 구독이 성사되지 않는 창이 생긴다.
/// 그 창에서 컷씬이 시작되면 가드가 뚫린다. static에는 그 창이 없다.
///
/// bool 토글이 아니라 깊이 카운터인 이유는 InputHandler._blockCount와 같다.
/// 안쪽 컷씬이 끝나도 바깥이 살아 있으면 게이트는 닫혀 있어야 한다.
/// </summary>
public static class CutsceneGate
{
    private static int _depth;
    private static int _skippableDepth;

    public static bool IsActive => 0 < _depth;

    public static bool IsSkippable => 0 < _skippableDepth;

    public static void Enter(bool isSkippable)
    {
        _depth++;
        if (isSkippable)
        {
            _skippableDepth++;
        }
    }

    public static void Exit(bool isSkippable)
    {
        _depth = Mathf.Max(0, _depth - 1);
        if (isSkippable)
        {
            _skippableDepth = Mathf.Max(0, _skippableDepth - 1);
        }
    }

    // 도메인 리로드를 끈 환경에서 이전 플레이의 값이 남는 것을 막는다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitStaticState()
    {
        _depth = 0;
        _skippableDepth = 0;
    }
}
