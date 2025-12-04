using UnityEngine;
using UnityEngine.Serialization;

namespace Framework
{
    public class ObjectUsingObjectPool : MonoBehaviour
    {
        [FormerlySerializedAs("MaxNum"),Range(1,100)] public int maxNum = 2;
    }
}
