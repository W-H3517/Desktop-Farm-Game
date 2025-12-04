using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Singleton base class for MonoBehaviour. Automatically loads itself; do not manually create any instances.
    /// </summary>
    /// <typeparam name="T">Class that inherits from the singleton base class.</typeparam>
    public abstract class SingletonAutoMono<T> : MonoBehaviour where T : SingletonAutoMono<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance is null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }
    }
}
