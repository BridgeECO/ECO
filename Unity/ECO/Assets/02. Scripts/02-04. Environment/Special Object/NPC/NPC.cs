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
    private List<NPCEventSO> _specialEvents;

    private PlayerInput _playerInput;
    private NPCSpecialEventQueue _specialEventQueue;
    private NPCEventExecutor _executor;

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

        if (_uiNPCDialogue != null && _uiNPCDialogue.IsDialogueOpen)
        {
            _uiNPCDialogue.AdvancePage();
            return;
        }

        if (_specialEventQueue.HasActiveEvent)
        {
            FireHighestPrioritySpecialEvent();
        }
        else
        {
            FireDefaultEvent();
        }
    }

    private void FireHighestPrioritySpecialEvent()
    {
        NPCEventSO eventData = _specialEventQueue.GetHighestPriority();
        if (eventData == null)
        {
            return;
        }

        FireEvent(eventData);
        _specialEventQueue.MarkFired(eventData);
    }

    private void FireDefaultEvent()
    {
        if (_defaultEvent == null)
        {
            return;
        }

        FireEvent(_defaultEvent);
    }

    private void FireEvent(NPCEventSO eventData)
    {
        if (eventData.DialogueLines != null && eventData.DialogueLines.Length > 0)
        {
            ShowDialogueWithCallback(eventData);
            return;
        }

        _executor.Execute(eventData, _playerInput);
    }

    private void ShowDialogueWithCallback(NPCEventSO eventData)
    {
        if (_uiNPCDialogue == null)
        {
            return;
        }

        _uiNPCDialogue.OnDialogueCompleted = null;
        _uiNPCDialogue.OnDialogueCompleted = () => _executor.Execute(eventData, _playerInput);
        _uiNPCDialogue.Open(eventData.DialogueLines);
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
