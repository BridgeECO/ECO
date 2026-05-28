using System.Collections.Generic;
using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "NPCEventSO", menuName = "ECO/NPC/Event Data")]
public class NPCEventSO : ScriptableObject
{
    [Foldout("Base")]
    [SerializeField]
    private string _eventId;

    [SerializeField]
    private ENPCEventType _eventType;

    [SerializeField]
    private int _priority;

    [Foldout("Dialogue")]
    [SerializeField]
    private string[] _dialogueLines;

    [SerializeField]
    private bool _hasChoices;

    [ShowIf("_hasChoices", true)]
    [SerializeField]
    private List<NPCChoiceOption> _choices;

    [ShowIf("_eventType", ENPCEventType.UnlockAbility)]
    [Foldout("Ability Unlock")]
    [SerializeField]
    private EPlayerUnlockableAbility _abilityToUnlock;

    [EndIf]
    [ShowIf("_eventType", ENPCEventType.ActivateGimmick)]
    [Foldout("Gimmick Activation")]
    [SerializeField]
    private List<TerrainObject> _targetTerrains;

    [SerializeField]
    private bool _activateGimmick;

    [EndIf]
    public string EventId => _eventId;
    public ENPCEventType EventType => _eventType;
    public int Priority => _priority;
    public string[] DialogueLines => _dialogueLines;
    public bool HasChoices => _hasChoices;
    public List<NPCChoiceOption> Choices => _choices;
    public EPlayerUnlockableAbility AbilityToUnlock => _abilityToUnlock;
    public List<TerrainObject> TargetTerrains => _targetTerrains;
    public bool ActivateGimmick => _activateGimmick;
}
