using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class NPC : SpecialObjectBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private GameObject _highlightObject;

    [SerializeField]
    private UI_NPCDialogue _uiNPCDialogue;

    [Foldout("Project")]
    [SerializeField]
    private NPCEventSO _defaultEvent;

    [SerializeField]
    private NPCEventSO _conditionMetDefaultEvent;

    [SerializeField]
    private List<NPCEventSO> _specialEvents;

    [Foldout("Hierarchy")]
    [SerializeField]
    [Tooltip("이벤트가 이름으로 지목할 수 있는 컷씬 목록입니다. ScriptableObject는 씬 오브젝트를 참조할 수 없습니다.")]
    private List<CutsceneDirector> _cutscenes = new List<CutsceneDirector>();

    private PlayerInput _playerInput;
    private NPCSpecialEventQueue _specialEventQueue;
    private NPCEventExecutor _executor;
    private NPCEventFlowRunner _flowRunner;
    private bool _isDefaultEventConditionMet;

    protected override void Awake()
    {
        base.Awake();
        _specialEventQueue = new NPCSpecialEventQueue();
        _executor = new NPCEventExecutor(_cutscenes);
        _flowRunner = new NPCEventFlowRunner(_uiNPCDialogue, _executor);
    }

    protected virtual void Start()
    {
        if (_specialEvents is not null)
        {
            for (int i = 0; i < _specialEvents.Count; i++)
            {
                if (_specialEvents[i] is not null)
                {
                    ActivateSpecialEvent(_specialEvents[i]);
                }
            }
        }
    }

    private void Update()
    {
        // 컷씬 중의 ESC는 스킵이다. 여기서 받아 버리면 컷씬이 최종 상태 확정 없이 죽는다.
        if (_flowRunner.IsInteracting && !CutsceneGate.IsActive && Input.GetKeyDown(KeyCode.Escape))
        {
            _flowRunner.CancelInteraction();
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(nameof(ETags.PlayerInteract)))
        {
            _playerInput = other.GetComponentInParent<PlayerInput>();
        }
        base.OnTriggerEnter2D(other);
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(nameof(ETags.PlayerInteract)))
        {
            _playerInput = null;
        }
        base.OnTriggerExit2D(other);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _flowRunner.CancelInteraction();
    }

    public override void ResetState()
    {
        base.ResetState();
        _specialEventQueue.ResetToSavedState();
        _flowRunner.CancelInteraction();
        RefreshHighlight(false);
    }

    public void SaveState()
    {
        _specialEventQueue.SaveFiredState();
    }

    public void ActivateSpecialEvent(NPCEventSO eventData)
    {
        _specialEventQueue.TryActivate(eventData);
    }

    public void DeactivateSpecialEvent(NPCEventSO eventData)
    {
        _specialEventQueue.TryDeactivate(eventData);
    }

    public void SetDefaultEvent(NPCEventSO eventData)
    {
        _defaultEvent = eventData;
    }

    public void SetDefaultEventConditionMet(bool isMet)
    {
        _isDefaultEventConditionMet = isMet;
    }

    protected override void HandlePlayerEnter()
    {
        RefreshHighlight(true);
    }

    protected override void HandlePlayerExit()
    {
        RefreshHighlight(false);
        _flowRunner.CancelInteraction();
    }

    protected override void Interact()
    {
        base.Interact();

        if (_flowRunner.IsInteracting)
        {
            return;
        }

        _flowRunner.StartInteraction(_defaultEvent, _conditionMetDefaultEvent, _isDefaultEventConditionMet, _specialEventQueue, _playerInput);
    }

    private void RefreshHighlight(bool isActive)
    {
        if (_highlightObject != null)
        {
            _highlightObject.SetActive(isActive);
        }
    }
}
