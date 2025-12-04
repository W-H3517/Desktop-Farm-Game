
namespace Framework
{
    /// <summary>
    /// Base class for the singleton pattern, provides a lock object for multithreaded synchronization
    /// </summary>
    /// <typeparam name="T">Class that inherits from the singleton base class</typeparam>
    public abstract class BaseManager<T> where T : class, new()
    {
        //懒汉单例模式使用时才会创建实例，节约内存，但是多线程不太安全，需要自己加锁
        private static T _instance;
        // ReSharper disable once StaticMemberInGenericType
        protected static readonly object Lock = new object();
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Lock)
                    {
                        if (_instance == null)
                        {
                            #region 想私有化无参构造可以这样使用（反射）
                
                            // 记得一移除，泛型约束的无参构造函数
                            // Type myType = typeof(T);
                            // ConstructorInfo constructorInfo = myType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                            //     null, Type.EmptyTypes, null);
                            // if (constructorInfo != null) _instance = constructorInfo.Invoke(null) as T;
                            // else Debug.LogError("没有对应的私有无参构造");

                            #endregion
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }
    
        //饿汉单例模式下，初始化只有一次，因为作为静态成员的初始值。但是任何地方一旦使用类名，不管是否真正用到实例，都会申请内存。容易浪费。
        #region 饿汉单例模式

        // private static T _instance = new T();
        // public static T Instance => _instance;

        #endregion
    }
}