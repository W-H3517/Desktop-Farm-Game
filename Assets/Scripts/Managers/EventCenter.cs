using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventCenter 
{
    private static EventCenter _instance = new EventCenter();
    public static EventCenter Instance => _instance;
    private EventCenter() { }
    
    private Dictionary<string, UnityAction<object>> _events = new Dictionary<string, UnityAction<object>>();
    
    public void AddEventListener(string eventName, UnityAction<object> action)
    {
        if (!_events.ContainsKey(eventName))
        {
            _events.Add(eventName, action);
        }
        else
        {
            _events[eventName] += action;
        }
    }
    
    public void RemoveEventListener(string eventName, UnityAction<object> action)
    {
        if (_events.ContainsKey(eventName))
        {
            _events[eventName] -= action;
        }
    }
    
    public void EventTrigger(string eventName, object eventData)
    {
        if (_events.ContainsKey(eventName))
        {
            _events[eventName]?.Invoke(eventData);
        }
    }
    
    /// <summary>
    /// 场景切换时用来清除无用Event
    /// </summary>
    public void EventsClear()
    {
        _events.Clear();
    }
}
