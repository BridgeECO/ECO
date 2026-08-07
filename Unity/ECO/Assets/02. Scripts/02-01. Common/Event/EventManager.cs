using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviourSingleton<EventManager>
{
    private readonly Action[] _events = new Action[(int)EEventType.MaxCount];
    // 페이로드가 있는 이벤트 채널. 같은 EEventType에 서로 다른 T를 섞어 쓰면
    // Broadcast 시 캐스팅이 실패하므로, 이벤트 하나당 페이로드 타입은 하나로 고정한다.
    private readonly Dictionary<EEventType, Delegate> _typedEvents = new Dictionary<EEventType, Delegate>();

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

    public void AddEventListener<T>(EEventType type, Action<T> action)
    {
        if (_typedEvents.TryGetValue(type, out Delegate existing))
        {
            if (existing is not Action<T>)
            {
                Debug.LogError($"Event type '{type}' already has a different payload type: {existing.GetType()}");
                return;
            }
            _typedEvents[type] = Delegate.Combine(existing, action);
            return;
        }
        _typedEvents[type] = action;
    }

    public void RemoveEventListener<T>(EEventType type, Action<T> action)
    {
        if (!_typedEvents.TryGetValue(type, out Delegate existing))
        {
            return;
        }

        // 타입이 다르면 Delegate.Remove가 조용히 무시해 해제 실패를 알아채지 못한다.
        if (existing is not Action<T>)
        {
            Debug.LogError($"Event type '{type}' payload mismatch. Expected {existing.GetType()}, got Action<{typeof(T)}>");
            return;
        }

        Delegate remaining = Delegate.Remove(existing, action);
        if (remaining == null)
        {
            _typedEvents.Remove(type);
            return;
        }
        _typedEvents[type] = remaining;
    }

    public void BroadcastEvent<T>(EEventType type, T payload)
    {
        if (!_typedEvents.TryGetValue(type, out Delegate existing))
        {
            return;
        }

        if (existing is not Action<T> action)
        {
            Debug.LogError($"Event type '{type}' payload mismatch. Expected {existing.GetType()}, got Action<{typeof(T)}>");
            return;
        }
        action.Invoke(payload);
    }

    private void ClearAllEvents()
    {
        for (int i = 0; i < EventCount; i++)
        {
            _events[i] = null;
        }
        _typedEvents.Clear();
    }
}
