using System;
using System.Collections.Generic;
using Data;
using LitJson;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DataMgr
{
    private static DataMgr _instance = new DataMgr();
    public static DataMgr Instance => _instance;
    public List<PlantInfo> AllPlantInfo = new ();
    
    
    private DataMgr()
    {
        AllPlantInfo = JsonMgr.Instance.LoadConfigurationData<List<PlantInfo>>("AllPlantInfo");
        foreach (PlantInfo plantInfo in AllPlantInfo)
        {
            AddressablesMgr.Instance.LoadResource("Plant", (AsyncOperationHandle<GameObject> handle) =>
            {
                GameObject.Instantiate(handle.Result).GetComponent<Plant>().Init(plantInfo);
            });
        }
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
