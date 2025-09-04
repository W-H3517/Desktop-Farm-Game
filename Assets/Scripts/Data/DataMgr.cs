using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class DataMgr
{
    private static DataMgr _instance = new DataMgr();
    public static DataMgr Instance => _instance;
    
    private DataMgr()
    {
        
    }
    
    public void LoadData()
    {
        
    }
    
    public void SaveData()
    {
        
    }
    
    public void Restart()
    {
        _instance = new DataMgr();
    }
}
