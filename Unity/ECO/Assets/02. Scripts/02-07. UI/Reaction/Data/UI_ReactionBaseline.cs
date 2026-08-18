using UnityEngine;

/// <summary>
/// 리액션이 건드리기 전의 원래 값 한 건. 채널에 따라 쓰는 칸이 다르다.
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
