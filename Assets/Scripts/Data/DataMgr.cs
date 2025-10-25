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
    public List<PlantBasicInfo> AllBasicPlantInfo ;
    public List<PlantsInScene> AllPlants ;
    public float WorldBottomLocation { get; }
    
    private DataMgr()
    {
        WorldBottomLocation = GetBottomDistance();
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
