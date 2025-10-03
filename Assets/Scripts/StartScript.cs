using System;
using System.Collections;
using Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class StartScript : MonoBehaviour
{
    private static StartScript _instance;
    public static StartScript Instance => _instance;
    
    private double _time = 0;
    
    
    void Start()
    {
        var datamgr = DataMgr.Instance;
        _instance = this;
    }
}
