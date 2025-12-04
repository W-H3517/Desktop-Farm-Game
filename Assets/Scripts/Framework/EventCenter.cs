using System;
using System.Collections.Generic;

namespace Framework
{
    public class EventCenter : BaseManager<EventCenter>
    {
        private Dictionary<E_EventList, Delegate>  _eventsDic = new Dictionary<E_EventList, Delegate>();

        public void EventTrigger<T>(E_EventList eventName, T parameter)
        {
            if (_eventsDic.TryGetValue(eventName, out var action))
            {
                (action as Action<T>)?.Invoke(parameter);
            }
        }

        public void AddEventListener<T>(E_EventList eventName, Action<T> listener)
        {
            if (_eventsDic.TryGetValue(eventName, out var oldAction))
            {
                _eventsDic[eventName] = (oldAction as Action<T>) + listener;
            }
            else
            {
                _eventsDic.Add(eventName, listener);
            }
        }

        public void RemoveEventListener<T>(E_EventList eventName, Action<T> listener)
        {
            if (_eventsDic.TryGetValue(eventName, out var value))
                _eventsDic[eventName] = (value as Action<T>) - listener;
        }

        /// <summary>
        /// Clear所有事件监听 
        /// </summary>
        public void Clear()
        {
            _eventsDic.Clear();
        }

        /// <summary>
        /// Clear特定事件的监听
        /// </summary>
        /// <param name="eventName">事件名</param>
        public void Clear(E_EventList eventName)
        {
            _eventsDic.Remove(eventName);
        }
    }
}
