using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventCenter 
{
    private static EventCenter _instance = new EventCenter();
    public static EventCenter Instance => _instance;
    private EventCenter() { }
    
    private Dictionary<string, Delegate> _events = new Dictionary<string, Delegate>();
    
    public void AddEventListener<T>(string eventName, UnityAction<T> action)
    {
        if (_events.TryGetValue(eventName, out var existing))
        {
            _events[eventName] = (UnityAction<T>)existing + action;
        }
        else
        {
            _events.Add(eventName, action);
        }
    }
    
    public void RemoveEventListener<T>(string eventName, UnityAction<T> action)
    {
        if (_events.TryGetValue(eventName, out var existing))
        {
            var newDel = (UnityAction<T>)existing - action;
            if (newDel == null)
                _events.Remove(eventName);
            else
                _events[eventName] = newDel;
        }
    }
    
    public void EventTrigger<T>(string eventName, T eventData)
    {
        if (_events.TryGetValue(eventName, out var existing))
        {
            (existing as UnityAction<T>)?.Invoke(eventData);
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
