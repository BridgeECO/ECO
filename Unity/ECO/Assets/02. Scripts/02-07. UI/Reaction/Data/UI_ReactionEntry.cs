using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>트리거 하나와 거기에 묶인 리액션 목록. 인스펙터에서 기획자가 다루는 단위다.</summary>
[Serializable]
public class UI_ReactionEntry
{
    // 리스트에 원소를 추가하면 유니티가 C# 초기값을 무시하고 0으로 채운다. 새 필드도 0이 정상 동작이어야 한다.
    [SerializeField]
    [Tooltip("인스펙터에서 항목을 구분하기 위한 이름입니다. 동작에는 영향이 없습니다.")]
    private string _label = string.Empty;

    [SerializeField]
    [Tooltip("체크하면 이 항목을 건너뜁니다.")]
    private bool _isMuted = false;

    [SerializeField]
    private EUIReactionTriggerKind _kind = EUIReactionTriggerKind.State;

    [SerializeField]
    private EUIReactionState _stateTrigger = EUIReactionState.Normal;

    [SerializeField]
    private EUIReactionEvent _eventTrigger = EUIReactionEvent.PointerEnter;

    [SerializeField]
    private EUIReactionSignal _signalTrigger = EUIReactionSignal.Show;

    [SerializeField]
    private EUIReactionInterruptPolicy _interruptPolicy = EUIReactionInterruptPolicy.Restart;

    [SerializeField]
    private EUIReactionExitPolicy _exitPolicy = EUIReactionExitPolicy.TweenToBaseline;

    [SerializeReference]
    [SubclassSelector]
    private List<UI_ReactionBase> _reactions = new List<UI_ReactionBase>();

    public bool IsEnabled => !_isMuted;
    public EUIReactionTriggerKind Kind => _kind;
    public EUIReactionState StateTrigger => _stateTrigger;
    public EUIReactionInterruptPolicy InterruptPolicy => _interruptPolicy;
    public EUIReactionExitPolicy ExitPolicy => _exitPolicy;
    public List<UI_ReactionBase> Reactions => _reactions;

    public bool MatchesState(EUIReactionState state)
    {
        return IsEnabled && _kind == EUIReactionTriggerKind.State && _stateTrigger == state;
    }

    public bool MatchesEvent(EUIReactionEvent uiEvent)
    {
        return IsEnabled && _kind == EUIReactionTriggerKind.Event && _eventTrigger == uiEvent;
    }

    public bool MatchesSignal(EUIReactionSignal signal)
    {
        return IsEnabled && _kind == EUIReactionTriggerKind.Signal && _signalTrigger == signal;
    }
}
