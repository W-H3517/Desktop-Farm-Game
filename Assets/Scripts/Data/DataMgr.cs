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
    public List<PlantBasicInfo> AllBasicPlantInfo ;
    public List<PlantsInScene> AllPlants ;
    
    
    private DataMgr()
    {
        AllBasicPlantInfo = JsonMgr.Instance.LoadConfigurationData<List<PlantBasicInfo>>("AllPlantBasicInfo");
        
        AllPlants = JsonMgr.Instance.LoadConfigurationData<List<PlantsInScene>>("AllPlants");
        
        foreach (PlantsInScene plant in AllPlants)
        {
            AddressablesMgr.Instance.LoadResource("Plant", (AsyncOperationHandle<GameObject> handle) =>
            {
                GameObject.Instantiate(handle.Result).GetComponent<Plant>().Init(plant);
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
