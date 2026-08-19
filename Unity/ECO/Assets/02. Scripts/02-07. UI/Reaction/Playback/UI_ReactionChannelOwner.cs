/// <summary>어떤 리액션이 (대상, 채널) 한 자리를 현재 쥐고 있는지. 자리를 뺏기면 Exit 정책대로 물러난다.</summary>
public struct UI_ReactionChannelOwner
{
    public int TargetId;
    public EUIReactionChannel Channel;
    public UI_ReactionBase Reaction;
    public int Priority;
    public EUIReactionInterruptPolicy InterruptPolicy;
    public EUIReactionExitPolicy ExitPolicy;
}
