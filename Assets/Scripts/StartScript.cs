using System;
using Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class StartScript : MonoBehaviour
{
    private static StartScript _instance;
    public static StartScript Instance => _instance;
    
    private Mouse mouse;
    private int _locationIndex;
    private Plant _preLoaded;
    
    void Awake()
    {
        _instance = this;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var datamgr = DataMgr.Instance;
        mouse = Mouse.current;
        AddNewPlant();
        // AddressablesMgr.Instance.LoadResource("Plant", (AsyncOperationHandle<GameObject> handle) =>
        // {
        //     _preLoaded = Instantiate(handle.Result).GetComponent<Plant>();
        //     _preLoaded.Init(new PlantInfo("Blueberry",2,0));
        //     // _preLoaded.Init("Blueberry_2");
        // } );
        // InputMgr.Instance.Enable(InputMgr.Instance.InputSystem.PlantMode.Get());
    }
    
    private Vector3 position ;
    // Update is called once per frame
    void Update()
    {
        position = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
        _locationIndex =  (int)Mathf.Floor(position.x)  + 20;
        // print(_locationIndex);
        _preLoaded?.PrePlace(_locationIndex);
    }
    /// <summary>
    /// 输入系统左键种植回调函数
    /// </summary>
    /// <param name="ctx"></param>
    public void PlantActionCallBack(InputAction.CallbackContext ctx)
    {
        _preLoaded.PlantAction();
        _preLoaded = null;
        InputMgr.Instance.Disable(InputMgr.Instance.InputSystem.PlantMode.Get());
    }

    public void AddNewPlant()
    {
        AddressablesMgr.Instance.LoadResource("Plant", (AsyncOperationHandle<GameObject> handle) =>
        {
            _preLoaded = Instantiate(handle.Result).GetComponent<Plant>();
            _preLoaded.Init(new PlantInfo("Blueberry",2,0));
            // _preLoaded.Init("Blueberry_2");
        } );
        InputMgr.Instance.Enable(InputMgr.Instance.InputSystem.PlantMode.Get());
    }
    
}
