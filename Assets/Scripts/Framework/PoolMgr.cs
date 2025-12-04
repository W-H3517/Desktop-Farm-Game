using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Framework
{
    public class PoolMgr : BaseManager<PoolMgr>
    {
        //todo:拓展自动回收父对象里受对象池管理的子对象
        //todo:拓展自动清理长期不使用的对象
        //todo：拓展支持AB包和Addressables系统
        //拓展资源加载（低优先级）
        
        private Dictionary<string, Drawer> _poolDict = new Dictionary<string, Drawer>();
        private GameObject _pool;

        private bool IsOpenLayout = true;
    
        private Dictionary<string,Stack<IRecyclable>> _recyclablesDict = new Dictionary<string, Stack<IRecyclable>>();
        

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 获取可回收类
        /// </summary>
        /// <typeparam name="T">可回收类</typeparam>
        /// <returns>可回收类对象</returns>
        public T GetRecyclable<T>() where T : class, IRecyclable, new()
        {
            string name = typeof(T).FullName;
            T obj;
            if (_recyclablesDict.TryGetValue(name, out var recyclableStack))
            {
                obj = recyclableStack.Count > 0 ? recyclableStack.Pop() as T : new T();
            }
            else
            {
                obj = new T();
                var stack = new Stack<IRecyclable>();
                _recyclablesDict.Add(name, stack);
            }
            if (obj == null)
                Debug.LogError("错误的对象类型");
            // Debug.Log("容器剩余：" + _recyclablesDict[name].Count + ". after getting " + name);
            return obj;
        }

        /// <summary>
        /// 将可回收类存放起来
        /// </summary>
        /// <param name="obj">可回收对象</param>
        /// <typeparam name="T">可回收类</typeparam>
        public void PutRecyclable<T>(T obj) where T : class, IRecyclable
        {
            if (obj == null)
            {
                Debug.LogError("传入对象为null，无法放入对象池");
                return;
            }
            //清空引用，释放不必要的垃圾
            obj.Reset();
            string name = typeof(T).FullName;
            // Debug.Log("PutRecyclable with" + name);
            if (_recyclablesDict.TryGetValue(name, out var recyclablesStack))
            {
                recyclablesStack.Push(obj);
            }
            else
            {
                Stack<IRecyclable> stack = new Stack<IRecyclable>();
                stack.Push(obj);
                _recyclablesDict.Add(name, stack);
            }
            // Debug.Log("容器剩余：" + _recyclablesDict[name].Count + ". after putting " + name);
        }
        
        /// <summary>
        /// 取出想用的对象
        /// </summary>
        /// <param name="pathName">Resources下的路径名</param>
        /// <param name="autoActive">是否自动激活，默认自动激活</param>
        /// <returns></returns>
        public GameObject GetGameObject(string pathName, bool autoActive = true)
        {
            #region 没有使用数量上线的逻辑

            // GameObject obj;
            // //从抽屉取
            // if (_poolDict.TryGetValue(pathName, out Drawer drawer) && drawer.Count > 0)
            // {
            //     obj = drawer.Pop();
            // }
            // else
            //     //没有可取的（抽屉或者内容），新建
            // {
            //     obj = GameObject.Instantiate(Resources.Load<GameObject>(pathName));
            //     obj.name = pathName;
            // }
            // return obj;

            #endregion

            //没池子的话新建池子
            if (IsOpenLayout)
                if (!_pool)  
                {
                    _pool = new GameObject("Pool");
                }

            GameObject obj;
            if (_poolDict.TryGetValue(pathName, out Drawer drawer))
            {
                obj = drawer.Pop(pathName, autoActive); //该方法封装了对有上限情况的逻辑处理。
            }
            else
            {
                obj = GameObject.Instantiate(Resources.Load<GameObject>(pathName));
                obj.name = pathName;
                Drawer tempDrawer;
                if (_pool)
                    tempDrawer = new Drawer(pathName, _pool.transform, true, obj);
                else
                    tempDrawer = new Drawer(pathName, null, true, obj);
                _poolDict.Add(pathName, tempDrawer);
            }
            return obj;
        }
    
        /// <summary>
        /// 放入暂时不用的对象
        /// </summary>
        /// <param name="gameObject">不用的对象</param>
        public void PutGameObject(GameObject gameObject)
        {
            //todo: 考虑放入对象名称规范化！
            
            //没池子的话新建池子
            if (IsOpenLayout) _pool ??= new GameObject("Pool");
        
            Drawer drawer; //抽屉容器
            //没有抽屉，则添加抽屉
            if (!_poolDict.ContainsKey(gameObject.name))
            {
                //抽屉名字与存放对象的名字一致
                if (_pool)
                    drawer = new Drawer(gameObject.name, _pool.transform, false, gameObject);
                else
                    drawer = new Drawer(gameObject.name, null, false, gameObject);
                _poolDict.Add(gameObject.name, drawer);
            }
            //获取已有抽屉
            else
            {
                drawer = _poolDict[gameObject.name];
            }
            //压入，内部写好了失活与层级变化
            drawer.Push(gameObject);
        }

        /// <summary>
        /// 主要在切换场景时，清空不需要的旧引用，防止内存泄露
        /// </summary>
        public void Clear()
        {
            _poolDict.Clear();
            _pool = null;
            _recyclablesDict.Clear();
        }
        
        /// <summary>
        /// 内部抽屉类，不想让别人使用
        /// </summary>
        private class Drawer
        {
            private Stack<GameObject> _items = new Stack<GameObject>();
            private GameObject _drawerObj;
            private List<GameObject> _usingItems = new List<GameObject>();

            public int Count => _items.Count;
            private int _maxNum = -1;
        
            /// <summary>
            /// 把暂时不用的对象放回池子。封装了失活对象、改变层级、从usingItems移除的操作（数量上线功能相关）
            /// </summary>
            /// <param name="obj">回收的对象</param>
            public void Push(GameObject obj)
            {
                //需要先隐藏或者失活，另一种方式是放在摄像机看不到的地方
                //先将对象失活
                obj.SetActive(false);
                //很重要，要检测是否已经放入，因为，有可能用户用同一对象多次请求放入！！！
                if(_items.Contains(obj))
                {
                    Debug.LogWarning("同一对象试图重复放入对象池，已被阻止，可能发生在一帧内同时触发回调函数，传入了同一对象");
                    return;
                }
                _items.Push(obj);
                RemoveUsingItems(obj);
                if (Instance.IsOpenLayout) obj.transform.SetParent(_drawerObj.transform,false);
            }
    
            // ReSharper disable Unity.PerformanceAnalysis
            /// <summary>
            /// 取出要使用的对象，该方法封装了对有实例数量上限时的相关复用逻辑
            /// </summary>
            /// <param name="pathName"></param>
            /// <param name="autoActive">是否自动激活</param>
            /// <returns>要使用的对象</returns>
            public GameObject Pop(string pathName, bool autoActive)
            {
                GameObject obj;
                //有剩余则直接获取
                if (_items.Count > 0)
                {
                    obj = _items.Pop();
                }
                //没有剩余时
                else
                {
                    //如果在使用中的对象达到数量上限，取出最老的
                    if (_usingItems.Count >= _maxNum )
                    {
                        // Debug.Log("重用");
                        //拿了最旧的，当作最新的使用，先取出，并失活，再在最后放入
                        obj = _usingItems[0];
                        _usingItems.RemoveAt(0);
                        obj.SetActive(false);   //这里先失活，以便后续重启
                    }
                    //没有达到使用上限，则实例化一个全新的
                    else
                    {
                        obj = GameObject.Instantiate(Resources.Load<GameObject>(pathName));
                        obj.name = pathName;
                    }
                }
                if (Instance.IsOpenLayout) obj.transform.SetParent(null,false);
            
                //取得对象后，在return之前做相关处理
                PushUsingItems(obj);    //记录
                obj.SetActive(autoActive);    //激活
                return obj;
            }
        
            /// <summary>
            /// 新建抽屉
            /// </summary>
            /// <param name="name">抽屉名字</param>
            /// <param name="parent">如果有的话，传入柜子对象</param>
            /// <param name="isGet">是否正在取用，是否记录到 using队列</param>
            /// <param name="obj">该抽屉存储对象的示例</param>
            public Drawer(string name, [CanBeNull] Transform parent, bool isGet, GameObject obj)
            {
                if (Instance.IsOpenLayout)
                {
                    _drawerObj = new GameObject(name);
                    _drawerObj.transform.SetParent(parent);
                }
                if (isGet) PushUsingItems(obj);
                
                ObjectUsingObjectPool info = obj.GetComponent<ObjectUsingObjectPool>();
                if (!info)
                {
                    Debug.LogError("该对象没有挂载ObjectUsingObjectPool脚本！");
                    _maxNum = 1;
                }
                else _maxNum = info.maxNum;
            }
        
            private void PushUsingItems(GameObject obj)
            {
                _usingItems.Add(obj);
                // Debug.Log(_usingItems.Count+" IN!");
            }

            private void RemoveUsingItems(GameObject obj)
            {
                // Debug.Log(_usingItems.Count + " OUT!");
                _usingItems.Remove(obj);
            }
        }
    }
}


