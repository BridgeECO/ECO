/// <summary>
/// 모션이 어디서 출발해 어디로 갈지의 기준.
/// </summary>
public enum EUIReactionValueMode
{
    /// <summary>기준값에 지정한 값을 곱하거나 더한 지점으로 간다. 배율 연출의 기본.</summary>
    RelativeToBaseline = 0,

    /// <summary>현재 값에서 지정한 절대값으로 간다.</summary>
    Absolute = 1,

    /// <summary>기준값에서 출발해 지정한 절대값으로 간다.</summary>
    FromBaseline = 2,

    /// <summary>현재 값을 그대로 출발점으로 삼는다. 이어붙이는 연출용.</summary>
    FromCurrent = 3,
}
