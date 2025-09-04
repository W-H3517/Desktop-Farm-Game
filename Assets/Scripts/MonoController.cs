using System.Collections;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// 真正的mono代理
/// </summary>
public class MonoController : MonoBehaviour
{
    private event UnityAction UpdateEvent; 
    
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    void Update()
    {
        if (UpdateEvent!= null)
        {
            UpdateEvent();
        }
    }

    public void AddUpdateEvent(UnityAction action)
    {
        UpdateEvent += action;
    }

    public void RemoveUpdateEvent(UnityAction action)
    {
        UpdateEvent -= action;
    }
}
