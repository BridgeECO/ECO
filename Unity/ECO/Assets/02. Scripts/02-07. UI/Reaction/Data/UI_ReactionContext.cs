using UnityEngine;

/// <summary>
/// 리액션이 재생에 필요한 주변 정보. 리액션은 순수 데이터라 매번 이걸로 넘겨받는다.
/// </summary>
public readonly struct UI_ReactionContext
{
    /// <summary>리액션이 대상을 지정하지 않았을 때 쓰는 기본 대상.</summary>
    public readonly GameObject Owner;

    public readonly UI_ReactionBaselineStore Baselines;

    public UI_ReactionContext(GameObject owner, UI_ReactionBaselineStore baselines)
    {
        Owner = owner;
        Baselines = baselines;
    }
}
