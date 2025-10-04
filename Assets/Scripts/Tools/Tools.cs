using UnityEngine;

namespace Tools
{
    public static class Tools
    {
        public static Transform GetParentObject(this Transform obj) 
        {
            if(obj.parent is null)
                return obj;
            return obj.parent.GetParentObject();
        }

        public static Vector2 LocationIndexToVector2(this int locationIndex)
        {
            return new Vector2(locationIndex-20f, -7f);
        }
    }
}