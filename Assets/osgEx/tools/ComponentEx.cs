using UnityEngine;

namespace osgEx 
{
    public static class ComponentEx
    {
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T value = obj.GetComponent<T>();
            if (value == null)
            {
                value = obj.AddComponent<T>();
            }
            return value;
        }
        public static T GetOrAddComponent<T>(this Component obj) where T : Component
        {
            T value = obj.GetComponent<T>();
            if (value == null)
            {
                value = obj.gameObject.AddComponent<T>();
            }
            return value;
        }
    }
}
