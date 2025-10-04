using Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PlantingMode
{
    public class PlantingModeMgr
    {
        public static PlantingModeMgr Instance => _instance;
        private static PlantingModeMgr _instance = new PlantingModeMgr();
        
        private Mouse _mouse;
        private Plant _preLoaded;
        
        private PlantingModeMgr()
        {
            _mouse = Mouse.current;
        }
        
        public void SelectPlaceCallBack(InputAction.CallbackContext ctx)
        {
            _preLoaded?.PrePlace((int)Mathf.Floor(Camera.main.ScreenToWorldPoint(_mouse.position.ReadValue()).x)  + 20);
        }
        
        /// <summary>
        /// 输入系统左键种植回调函数
        /// </summary>
        /// <param name="ctx"></param>
        public void PlantActionCallBack(InputAction.CallbackContext ctx)
        {
            if (TransparentWindow.Instance != null && TransparentWindow.Instance.uiHit) return;
            //_preLoaded.transform.position坐标在左下角，可能不包括collider，因此加一个offset
            if (Physics2D.OverlapPoint(_preLoaded.transform.position+Vector3.one*0.5f, LayerMask.GetMask("Plant")))
            {
                return;
            }
            _preLoaded.PlantAction();
            _preLoaded = null;
            InputMgr.Instance.Disable(InputMgr.Instance.InputSystem.PlantMode.Get());
        }

        public void AddNewPlant(int basicInfoID)
        {
            if (_preLoaded != null) return;
            AddressablesMgr.Instance.LoadResource("Plant", (AsyncOperationHandle<GameObject> handle) =>
            {
                _preLoaded = GameObject.Instantiate(handle.Result).GetComponent<Plant>();
                _preLoaded.Init(new PlantsInScene(basicInfoID,4,0));
                _preLoaded.gameObject.layer = LayerMask.NameToLayer("PrePlanting");
            } );
            InputMgr.Instance.Enable(InputMgr.Instance.InputSystem.PlantMode.Get());
        }
    }
}