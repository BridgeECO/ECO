using System.Collections.Generic;
using UnityEngine;
using VInspector;

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

    // ScriptableObject는 씬 오브젝트를 참조할 수 없다. NPC가 인스펙터로 들고 있는
    // 컷씬 목록에서 이 이름으로 찾는다.
    [ShowIf(nameof(_eventType), ENPCEventType.PlayCutscene)]
    [SerializeField]
    private string _cutsceneId;
    [EndIf]

    public string EventId => _eventId;
    public ENPCEventType EventType => _eventType;
    public int Priority => _priority;
    public string[] DialogueLines => _dialogueLines;
    public bool HasChoices => _hasChoices;
    public List<NPCChoiceOption> Choices => _choices;
    public EPlayerUnlockableAbility AbilityToUnlock => _abilityToUnlock;
    public List<TerrainObject> TargetTerrains => _targetTerrains;
    public bool IsGimmickActive => _isGimmickActive;
    public string CutsceneId => _cutsceneId;
}
