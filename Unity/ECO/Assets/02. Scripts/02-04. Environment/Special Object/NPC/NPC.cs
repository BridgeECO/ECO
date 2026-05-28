using System.Collections.Generic;
using UnityEngine;
using VInspector;
using Cysharp.Threading.Tasks;

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

    private PlayerInput _playerInput;
    private NPCSpecialEventQueue _specialEventQueue;
    private NPCEventExecutor _executor;
    
    private bool _isInteracting;
    private bool _isDefaultEventConditionMet;

    public void SetDefaultEventConditionMet(bool isMet)
    {
        _isDefaultEventConditionMet = isMet;
    }

    protected override void Awake()
    {
        base.Awake();
        _specialEventQueue = new NPCSpecialEventQueue();
        _executor = new NPCEventExecutor();
    }

    protected virtual void Start()
    {
        if (_specialEvents != null)
        {
            for (int i = 0; i < _specialEvents.Count; i++)
            {
                if (_specialEvents[i] != null)
                {
                    ActivateSpecialEvent(_specialEvents[i]);
                }
            }
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        HideDialogue();
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

    public override void ResetState()
    {
        base.ResetState();
        _specialEventQueue.ResetToSavedState();
        HideDialogue();
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

    protected override void HandlePlayerEnter()
    {
        RefreshHighlight(true);
    }

    protected override void HandlePlayerExit()
    {
        RefreshHighlight(false);
        HideDialogue();
    }

    protected override void Interact()
    {
        base.Interact();

        if (_isInteracting)
        {
            if (_uiNPCDialogue != null && _uiNPCDialogue.IsDialogueOpen)
            {
                _uiNPCDialogue.AdvancePage();
            }
            return;
        }

        InteractAsync().Forget();
    }

    private async UniTaskVoid InteractAsync()
    {
        _isInteracting = true;

        try
        {
            if (_specialEventQueue.HasActiveEvent)
            {
                await FireHighestPrioritySpecialEventAsync();
            }
            else
            {
                await FireDefaultEventAsync();
            }
        }
        finally
        {
            _isInteracting = false;
        }
    }

    private async UniTask FireHighestPrioritySpecialEventAsync()
    {
        NPCEventSO eventData = _specialEventQueue.GetHighestPriority();
        if (eventData == null)
        {
            return;
        }

        bool isExecuted = await ExecuteEventFlowAsync(eventData);
        if (isExecuted)
        {
            _specialEventQueue.MarkFired(eventData);
        }

        HideDialogue();
    }

    private async UniTask FireDefaultEventAsync()
    {
        NPCEventSO targetEvent = _isDefaultEventConditionMet && _conditionMetDefaultEvent != null ? _conditionMetDefaultEvent : _defaultEvent;

        if (targetEvent == null)
        {
            return;
        }

        await ExecuteEventFlowAsync(targetEvent);
        HideDialogue();
    }

    private async UniTask<bool> ExecuteEventFlowAsync(NPCEventSO eventData)
    {
        if (eventData == null)
        {
            return false;
        }

        NPCEventSO nextEvent = eventData;
        bool isExecuted = false;

        while (nextEvent != null)
        {
            if (nextEvent.DialogueLines != null && nextEvent.DialogueLines.Length > 0)
            {
                NPCEventSO chosenEvent = await ShowDialogueAndGetChoiceAsync(nextEvent);

                if (chosenEvent == null)
                {
                    return false;
                }

                if (chosenEvent != nextEvent)
                {
                    nextEvent = chosenEvent;
                    continue;
                }
            }

            _executor.Execute(nextEvent, _playerInput);
            isExecuted = true;
            break;
        }

        return isExecuted;
    }

    private async UniTask<NPCEventSO> ShowDialogueAndGetChoiceAsync(NPCEventSO eventData)
    {
        if (_uiNPCDialogue == null)
        {
            return eventData;
        }

        var tcs = new UniTaskCompletionSource<NPCEventSO>();

        _uiNPCDialogue.OnDialogueCompleted = null;
        _uiNPCDialogue.OnDialogueCompleted = () =>
        {
            if (eventData.HasChoices && eventData.Choices != null && eventData.Choices.Count > 0)
            {
                _uiNPCDialogue.ShowChoices(eventData.Choices, (selectedEvent) =>
                {
                    tcs.TrySetResult(selectedEvent);
                });
            }
            else
            {
                tcs.TrySetResult(eventData);
            }
        };

        _uiNPCDialogue.Open(eventData.DialogueLines);

        return await tcs.Task;
    }

    private void HideDialogue()
    {
        if (_uiNPCDialogue != null)
        {
            _uiNPCDialogue.Close();
        }
    }

    private void RefreshHighlight(bool isActive)
    {
        if (_highlightObject != null)
        {
            _highlightObject.SetActive(isActive);
        }
    }
}
