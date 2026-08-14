/// <summary>
/// 지속 상태가 끝났을 때 어떻게 물러날지. 이 처리가 없으면 마우스를 빠르게 움직였을 때
/// 연출이 중간값에 멈춘 채 남는다.
/// </summary>
public enum EUIReactionExitPolicy
{
    /// <summary>지정한 시간과 커브로 기준값까지 되돌린다.</summary>
    TweenToBaseline = 0,

    /// <summary>즉시 기준값을 대입한다.</summary>
    Instant = 1,

    /// <summary>재생 중인 연출을 현재 지점부터 거꾸로 돌린다.</summary>
    Reverse = 2,

    /// <summary>끝까지 재생한 뒤 기준값으로 돌아간다.</summary>
    PlayToEnd = 3,

    /// <summary>되돌리지 않고 현재 값을 유지한다.</summary>
    Keep = 4,
}
