using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

[DisallowMultipleComponent]
public class ResourceUser: MonoBehaviour
{
    private UnityAction _release;

    public void Load<T>(string resourceName, Action<AsyncOperationHandle<T>> cb) where T : UnityEngine.Object
    {
        AddressablesMgr.Instance.LoadResource(resourceName, cb);
        _release += () =>
        {
            AddressablesMgr.Instance.ReleaseResource<T>(resourceName);
        };
    }
    
    void OnDestroy()
    {
        _release?.Invoke();
        _release = null;
    }
}