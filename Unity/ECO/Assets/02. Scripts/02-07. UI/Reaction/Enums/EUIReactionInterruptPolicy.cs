/// <summary>
/// 재생이 끝나기 전에 같은 리액션이 다시 요청됐을 때의 처리.
/// UI 입력은 짧은 시간에 반복해서 들어오므로 기본값은 현재 값에서 새 목표로 이어 가는 Restart다.
/// </summary>
public enum EUIReactionInterruptPolicy
{
    /// <summary>현재 값에서 새 목표값으로 다시 시작한다.</summary>
    Restart = 0,

    /// <summary>재생 중인 연출을 거꾸로 돌린다.</summary>
    Reverse = 1,

    /// <summary>끝까지 재생한 뒤 새 요청을 실행한다.</summary>
    PlayToEndThenPlay = 2,

    /// <summary>끝날 때까지 새 요청을 버린다.</summary>
    IgnoreUntilDone = 3,

    /// <summary>같은 리액션이 이미 돌고 있으면 아무것도 하지 않는다.</summary>
    SkipIfSame = 4,
}
