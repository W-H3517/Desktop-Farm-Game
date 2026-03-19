using System.Collections;
using Data;
using Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PlantingMode
{
    public class PlantingModeMgr : BaseManager<PlantingModeMgr>
    {
        private Mouse _mouse;
        private Plant _preLoaded;
        private int _cursorLocationIndex; //记录的“鼠标” 位置。
        private bool _isHolding = false; //记录是否处于按下状态，提供连续种植功能

        public PlantingModeMgr()
        {
            _mouse = Mouse.current;
        }

        /// <summary>
        /// 种植模式滑动鼠标回调函数，实时更新位置
        /// </summary>
        /// <param name="ctx"></param>
        public void SelectPlaceCallBack(InputAction.CallbackContext ctx)
        {
            _cursorLocationIndex = (int)Mathf.Floor(Camera.main.ScreenToWorldPoint(_mouse.position.ReadValue()).x) + 20;
            _preLoaded?.PrePlace(_cursorLocationIndex);
        }

        /// <summary>
        /// 输入系统左键种植回调函数
        /// </summary>
        /// <param name="ctx"></param>
        public void PlantActionCallBack(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                _isHolding = true;
                MonoMgr.Instance.StartCoroutine(Planting());
            }
            else if (ctx.canceled)
            {
                _isHolding = false;
            }
        }

        IEnumerator Planting()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (_isHolding) //按住状态，连续种植
            {
                if (DesktopWindowController.Primary != null && DesktopWindowController.Primary.IsPointerOverUI)
                {
                    yield return waitForFixedUpdate; //continue之前将控制权交回unity，避免死循环。
                    continue;
                }

                //_preLoaded.transform.position坐标在左下角，可能不包括collider，因此加一个offset
                // if (Physics2D.OverlapPoint(_preLoaded.transform.position + Vector3.one * 0.5f, LayerMask.GetMask("Plant")))
                // {
                //     return;
                // }
                //重写检测逻辑，使用性能更低的数据层面检测，而不是每次都用射线检测！
                if (DataMgr.Instance.AllPlants[_preLoaded.LocationIndex] != null)
                {
                    Debug.LogWarning("Location occupied");
                    yield return waitForFixedUpdate; //continue之前将控制权交回unity，避免死循环。
                    continue;
                }

                _preLoaded.PlantAction();
                int basicINfoID = _preLoaded.BasicInfoID;
                _preLoaded = null;
                AddNewPlantButton(basicINfoID); //放置完成后，立马添加新的待放置物品。
                yield return waitForFixedUpdate; //等待一帧，为了连续种植。
            }
        }

        /// <summary>
        /// 退出种植模式的按键回调函数
        /// </summary>
        /// <param name="ctx"></param>
        public void QuitPlantingModeCallBack(InputAction.CallbackContext ctx)
        {
            GameObject.Destroy(_preLoaded?.gameObject);
            _preLoaded = null;
            InputMgr.Instance.Disable(InputMgr.Instance.InputSystem.PlantMode.Get());
        }

        /// <summary>
        /// 添加一个新的种子在种植模式中（种植模式的开始）
        /// </summary>
        /// <param name="basicInfoID"></param>
        public void AddNewPlantButton(int basicInfoID)
        {
            if (_preLoaded != null) //如果_preLoaded未置空，说明是异常开启新的种植模式，先结束上一次
            {
                InputMgr.Instance.Disable(InputMgr.Instance.InputSystem.PlantMode.Get());
                GameObject.Destroy(_preLoaded.gameObject);
                _preLoaded = null;
            }

            AddressablesMgr.Instance.LoadResource("Plant", (AsyncOperationHandle<GameObject> handle) =>
            {
                _preLoaded = GameObject.Instantiate(handle.Result).GetComponent<Plant>();
                _preLoaded.Init(new PlantsInScene(basicInfoID, 4, _cursorLocationIndex), true);
                _preLoaded.gameObject.layer = LayerMask.NameToLayer("PrePlanting");
            });
            InputMgr.Instance.Enable(InputMgr.Instance.InputSystem.PlantMode.Get());
        }
    }
}
