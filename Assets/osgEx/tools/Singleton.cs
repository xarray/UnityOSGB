using System;
using UnityEngine;

namespace osgEx 
{
    public class Singleton<T> where T : Singleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = (T)Activator.CreateInstance(typeof(T), true);
                        }
                    }
                }
                return _instance;
            }
        }
    }
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        protected virtual bool DestroyOnLoadScene { get { return false; } }
        private static T _instance;
        private static readonly object _lock = new object();
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance = GameObject.FindObjectOfType<T>();
                        if (_instance == null)
                        {
                            GameObject gameObject = new GameObject();
                            _instance = gameObject.AddComponent<T>();
                            Instance.name = typeof(T).Name;
                            if (!_instance.DestroyOnLoadScene)
                            {
                                GameObject.DontDestroyOnLoad(_instance);
                            }
                        }
                    }
                }
                return _instance;
            }
        }
        protected virtual void __Awake() { }
        protected virtual void __OnDestroy() { }
        private void Awake()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = this as T;
                }
                else if (_instance != this)
                {
                    Destroy(this);
                }
            }
            __Awake();
        }
        private void OnDestroy()
        {
            lock (_lock)
            {
                if (_instance == this)
                {
                    _instance = null;
                }
            }
            __OnDestroy();
        }
    }
}
