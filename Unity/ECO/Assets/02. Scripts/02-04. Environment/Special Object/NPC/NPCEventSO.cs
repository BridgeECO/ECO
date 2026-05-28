using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCEventSO", menuName = "ECO/NPC/Event Data")]
public class NPCEventSO : ScriptableObject
{
    [SerializeField]
    private string _eventId;

    [SerializeField]
    private ENPCEventType _eventType;

    [SerializeField]
    private int _priority;

    [SerializeField]
    private string[] _dialogueLines;

    [SerializeField]
    private bool _hasChoices;

    [SerializeField]
    private List<NPCChoiceOption> _choices;

    [SerializeField]
    private EPlayerUnlockableAbility _abilityToUnlock;

    [SerializeField]
    private List<TerrainObject> _targetTerrains;

    [SerializeField]
    private bool _isGimmickActive;

    public string EventId => _eventId;
    public ENPCEventType EventType => _eventType;
    public int Priority => _priority;
    public string[] DialogueLines => _dialogueLines;
    public bool HasChoices => _hasChoices;
    public List<NPCChoiceOption> Choices => _choices;
    public EPlayerUnlockableAbility AbilityToUnlock => _abilityToUnlock;
    public List<TerrainObject> TargetTerrains => _targetTerrains;
    public bool IsGimmickActive => _isGimmickActive;
}
