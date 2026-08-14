using UnityEngine;

/// <summary>
/// 리액션이 건드리기 전의 원래 값 한 건. 채널에 따라 쓰는 칸이 다르다.
/// 채널 수가 적고 한 Reactor가 가지는 항목이 10개 안팎이라, 칸을 나누는 대신
/// 한 구조체에 모아 두고 선형 탐색한다(딕셔너리 키 박싱 회피).
/// </summary>
public struct UI_ReactionBaseline
{
    public int TargetId;
    public EUIReactionChannel Channel;

    /// <summary>Position·Rotation·Scale은 xyz, Color는 rgba, Alpha는 x를 쓴다.</summary>
    public Vector4 Value;

    /// <summary>Sprite·Material 원본.</summary>
    public Object Reference;

    /// <summary>Active·ComponentEnable 원본.</summary>
    public bool Flag;
}
