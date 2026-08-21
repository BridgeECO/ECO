using System;
using UnityEngine;

/// <summary>
/// 값을 랜덤하게 떨리게 하는 리액션의 공통 뼈대.
/// 감쇠와 반복(Loops)을 어떻게 조합하느냐로 단발 흔들림과 계속 떠는 연출이 갈린다.
/// </summary>
[Serializable]
public abstract class UI_ShakeReactionBase : UI_TweenReactionBase
{
    [SerializeField]
    [Min(1)]
    [Tooltip("떨리는 동안 방향이 바뀌는 횟수입니다. 클수록 잘게 떱니다.")]
    private int _vibrato = 10;

    [SerializeField]
    [Range(0f, 180f)]
    [Tooltip("방향이 얼마나 제멋대로 튈지입니다. 0에 가까울수록 한 방향으로만 떱니다.")]
    private float _randomness = 90f;

    [SerializeField]
    [Tooltip("체크하면 떨림이 점점 잦아들며 끝납니다. 계속 떠는 연출은 이 체크를 끄고 반복을 -1로 둡니다.")]
    private bool _isFadeOut = true;

    protected int Vibrato => _vibrato;

    protected float Randomness => _randomness;

    protected bool IsFadeOut => _isFadeOut;

    /// <summary>
    /// 흔들림에는 시작값이 없다. 기준값에서 출발하는 모드일 때만 출발점을 기준값으로 맞춰,
    /// 다른 연출이 남겨 둔 값 위에서 떨기 시작하지 않게 한다.
    /// </summary>
    protected bool IsStartFromBaseline => Motion.ValueMode == EUIReactionValueMode.FromBaseline;
}
