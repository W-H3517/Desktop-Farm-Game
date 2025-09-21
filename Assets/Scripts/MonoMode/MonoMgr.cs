using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MonoMgr
{
    private static MonoMgr _instance = new MonoMgr();
    public static MonoMgr Instance => _instance;
    
    private MonoController _controller;

    private MonoMgr()
    {
        //实例化真正的mono代理，并存在内部的_controller变量
        var obj = new GameObject("MonoController");
        _controller = obj.AddComponent<MonoController>();
    }
    
    public void AddUpdateEvent(UnityAction action)
    {
        _controller.AddUpdateEvent(action);
    }
    
    public void RemoveUpdateEvent(UnityAction action)
    {
        _controller.RemoveUpdateEvent(action);
    }
    
    public Coroutine StartCoroutine(IEnumerator routine)
    {
        return _controller.StartCoroutine(routine);
    }
    
    public void StopCoroutine(Coroutine routine)
    {
        _controller.StopCoroutine(routine);
    }
}