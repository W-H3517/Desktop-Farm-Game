using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesMgr
{
    public static AddressablesMgr Instance => _instance;
    private static AddressablesMgr _instance = new AddressablesMgr();
    private AddressablesMgr() { }
    
    // 用一个 Entry 把句柄与计数放在一起，便于扩展（如 generation）
    private class Entry
    {
        public AsyncOperationHandle Handle;
        public int RefCount;
        // public int Generation;   // 抑制过期回调（释放/重载后递增）
    }
    private readonly Dictionary<string, Entry> _map = new();
    private static string MakeKey<T>(string name) where T : UnityEngine.Object
        => $"{name}_{typeof(T).Name}";
    
    public void LoadResource<T>(string resourceName, Action<AsyncOperationHandle<T>> onSuccess)
        where T : UnityEngine.Object
    {
        var key = MakeKey<T>(resourceName);
        if (_map.TryGetValue(key, out Entry h))
        {
            h.RefCount++;
            if (h.Handle.IsDone)
            {
                if (h.Handle.Status == AsyncOperationStatus.Succeeded)
                {
                    onSuccess(h.Handle.Convert<T>());
                }
                else
                {
                    Debug.LogError($"Load resource failed: {resourceName}\n{h.Handle.OperationException}");
                }
            }
            else
            {
                h.Handle.Convert<T>().Completed += (obj) =>
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        onSuccess(obj);
                    }
                    else
                    {
                        Debug.LogError($"Load resource failed: {resourceName}\n{obj.OperationException}");
                    }
                };
            }
        }
        else
        {
            var handle = Addressables.LoadAssetAsync<T>(resourceName);
            _map.Add(key, new Entry { RefCount = 1 , Handle = handle });
            handle.Completed += (obj) =>
            {
                if (obj.Status == AsyncOperationStatus.Succeeded)
                {
                    onSuccess(obj);
                }
                else
                {
                    Debug.LogError($"Load resource failed: {resourceName}\n{obj.OperationException}");
                }
            };
        }
    }

    public void ReleaseResource<T>(string resourceName) where T : UnityEngine.Object
    {
        var key = MakeKey<T>(resourceName);
        if (_map.TryGetValue(key, out var entry))
        {
            if (--entry.RefCount == 0)
            {
                Addressables.Release(entry.Handle);
                _map.Remove(key);
            }
        }
    }
}