using System;
using UnityEngine;

public class EventManager : MonoBehaviourSingleton<EventManager>
{
    private readonly Action[] _events = new Action[(int)EEventType.MaxCount];

    private int EventCount => _events.Length;

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
