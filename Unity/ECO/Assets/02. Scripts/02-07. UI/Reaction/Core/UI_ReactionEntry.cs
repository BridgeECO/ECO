using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 트리거 하나와 거기에 묶인 리액션 목록. 인스펙터에서 기획자가 다루는 단위다.
///
/// 필드 기본값 주의: 리스트에 원소를 추가하면 유니티가 필드를 0으로 채우고 C# 초기값은
/// 무시한다. 그래서 켜짐/꺼짐 같은 값은 0이 정상 동작이 되도록 뒤집어 두고, 열거형도
/// 기본으로 쓸 항목을 0번에 배치한다. 그러지 않으면 방금 추가한 항목이 조용히 죽는다.
/// </summary>
[Serializable]
public class UI_ReactionEntry
{
    [SerializeField]
    [Tooltip("인스펙터에서 항목을 구분하기 위한 이름입니다. 동작에는 영향이 없습니다.")]
    private string _label = string.Empty;

    [SerializeField]
    [Tooltip("체크하면 이 항목을 건너뜁니다.")]
    private bool _isMuted = false;

    [SerializeField]
    private EUIReactionTriggerKind _kind = EUIReactionTriggerKind.State;

    [SerializeField]
    private EUIReactionState _stateTrigger = EUIReactionState.Hover;

    [SerializeField]
    private EUIReactionEvent _eventTrigger = EUIReactionEvent.Activate;

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
