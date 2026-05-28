using System.Collections.Generic;

public class NPCSpecialEventQueue
{
    private readonly List<NPCEventSO> _activeEvents = new List<NPCEventSO>();
    private readonly HashSet<string> _firedEventIds = new HashSet<string>();
    private readonly HashSet<string> _savedFiredEventIds = new HashSet<string>();

    public bool HasActiveEvent => _activeEvents.Count > 0;

    public bool TryActivate(NPCEventSO eventData)
    {
        if (_firedEventIds.Contains(eventData.EventId))
        {
            return false;
        }

        if (_activeEvents.Contains(eventData))
        {
            return false;
        }

        InsertSorted(eventData);
        return true;
    }

    public bool TryDeactivate(NPCEventSO eventData)
    {
        return _activeEvents.Remove(eventData);
    }

    public NPCEventSO GetHighestPriority()
    {
        if (_activeEvents.Count == 0)
        {
            return null;
        }

        return _activeEvents[0];
    }

    public void MarkFired(NPCEventSO eventData)
    {
        _activeEvents.Remove(eventData);
        _firedEventIds.Add(eventData.EventId);
    }

    public void SaveFiredState()
    {
        _savedFiredEventIds.Clear();
        foreach (string id in _firedEventIds)
        {
            _savedFiredEventIds.Add(id);
        }
    }

    public void ResetToSavedState()
    {
        _firedEventIds.Clear();
        foreach (string id in _savedFiredEventIds)
        {
            _firedEventIds.Add(id);
        }
        _activeEvents.Clear();
    }

    private void InsertSorted(NPCEventSO eventData)
    {
        int insertIndex = _activeEvents.Count;
        for (int i = 0; i < _activeEvents.Count; i++)
        {
            if (eventData.Priority > _activeEvents[i].Priority)
            {
                insertIndex = i;
                break;
            }
        }
        _activeEvents.Insert(insertIndex, eventData);
    }
}
