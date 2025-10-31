using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Data;
using LitJson;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DataMgr
{
    private static DataMgr _instance = new DataMgr();
    public static DataMgr Instance => _instance;
    public readonly PlantBasicInfo[] AllBasicPlantInfo;

    //涉及到排序操作，封装起来更安全
    // private readonly List<PlantsInScene> allPlants = new List<PlantsInScene>();
    // public IReadOnlyList<PlantsInScene> AllPlants => allPlants;

    public PlantsInScene[] AllPlants = new PlantsInScene[40];
    public float WorldBottomLocation { get; }
    
    private DataMgr()
    {
        WorldBottomLocation = GetBottomDistance();
        AllBasicPlantInfo = JsonMgr.Instance.LoadConfigurationData<PlantBasicInfo[]>("AllPlantBasicInfo");
        
        var allPlants = JsonMgr.Instance.LoadConfigurationData<List<PlantsInScene>>("AllPlants");
        
        foreach (PlantsInScene plant in allPlants)
        {
            //放在数组对应位置
            AllPlants[plant.LocationIndex] = plant;
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

    /// <summary>
    /// Add plant object to DataMgr by its location index.
    /// </summary>
    /// <param name="plant"></param>
    public void AddPlant(PlantsInScene plant)
    {
        AllPlants[plant.LocationIndex] = plant;
    }

    public void Restart()
    {
        _instance = new DataMgr();
    }
    
    //获取任务栏高度相关逻辑
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
        
    private enum ABEdge : uint
    {
        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3
    }
        
    [StructLayout(LayoutKind.Sequential)]
    private struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public ABEdge uEdge;
        public RECT rc;
        public int lParam;
    }
        
    [DllImport("shell32.dll", SetLastError = true)]
    private static extern uint SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);
    private const int ABM_GETTASKBARPOS = 5;
        
    private float GetBottomDistance()
    {
        APPBARDATA data = new APPBARDATA();
        data.cbSize = (uint)Marshal.SizeOf(data);
        SHAppBarMessage(ABM_GETTASKBARPOS, ref data);
        int taskbarHeightPx;
        if (data.uEdge == ABEdge.Bottom )
            taskbarHeightPx= data.rc.Bottom - data.rc.Top;
        else
            taskbarHeightPx= 0;
        float normalizedHeight = 1.0f * taskbarHeightPx / Screen.height ;
        float worldHeight = Camera.main.orthographicSize * 2;
        return worldHeight * normalizedHeight - Camera.main.orthographicSize;
    }
}
