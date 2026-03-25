using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviourSingleton<EventManager>
{
    private List<Action> _events = new List<Action>((int)EEventType.MaxCount);

    private int EventCount => _events.Count;

    private void OnDestroy()
    {
        ClearAllEvents();
    }

    public void AddEventListener(EEventType type, Action action)
    {
        int index = (int)type;
        if (EventCount <= index)
        {
            Debug.LogError($"Invalid event type : {type}");
            return;
        }
        _events[index] += action;
    }

    public void RemoveEventListener(EEventType type, Action action)
    {
        int index = (int)type;
        if (EventCount <= index)
        {
            Debug.LogError($"Invalid event type : {type}");
            return;
        }
        _events[index] -= action;
    }

    public void BroadcastEvent(EEventType type)
    {
        int index = (int)type;
        if (EventCount <= index)
        {
            Debug.LogError($"Invalid event type : {type}");
            return;
        }
        _events[index]?.Invoke();
    }

    private void ClearAllEvents()
    {
        for (int i = 0; i < EventCount; i++)
        {
            _events[i] = null;
        }
    }
}
