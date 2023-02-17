
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public class TransformEx : MonoBehaviour
    {
        public Action onChanged;
        public bool hasChanged { get; private set; }
        Transform thisTransform;
        private void Awake()
        {
            this.hideFlags = HideFlags.HideInInspector;
            this.thisTransform = transform;
        }
        private void LateUpdate()
        {
            hasChanged = thisTransform.hasChanged;
            if (hasChanged)
            {
                onChanged?.Invoke();
                thisTransform.hasChanged = false;
            }
        }
    }
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

    public class osgManager : MonoSingleton<osgManager>
    {
        private int frameCount;
        private Plane[] m_calculateFrustumPlanes;
        private bool needUpdateCalculateFrustumPlanes;
        public Plane[] CalculateFrustumPlanes
        {
            get
            {
                if (needUpdateCalculateFrustumPlanes && frameCount < Time.frameCount)
                {
                    GeometryUtility.CalculateFrustumPlanes(Camera.main, m_calculateFrustumPlanes);
                    frameCount = Time.frameCount;
                    needUpdateCalculateFrustumPlanes = false;
                }
                return m_calculateFrustumPlanes;
            }
        }
        [SerializeField]
        private Material m_templeteMaterial;
        public Material templeteMaterial { get {
                if (m_templeteMaterial==null)
                {
                    if (QualitySettings.renderPipeline == null)
                    {
                        m_templeteMaterial = new Material(Shader.Find("Standard"));
                    }
                    else
                    {
                        m_templeteMaterial = new Material(Shader.Find("Lit"));
                    }
                }
                return m_templeteMaterial;
            } }
        private void OnCameraMove()
        {
            m_cameraStopTime = 0;
            needUpdateCalculateFrustumPlanes = true;
        }
        protected override void __Awake()
        {
            base.__Awake();
            m_calculateFrustumPlanes = new Plane[6];
        }
//#if UNITY_EDITOR
        public string __rootPath;
        public string[] __filePath;
        private void Start()
        {
            Camera.main.GetOrAddComponent<TransformEx>().onChanged += OnCameraMove;
            if (string.IsNullOrEmpty(__rootPath)||__filePath==null)
            {
                return;
            }
            LoadOSGB(__rootPath, __filePath);
        }
//#else
        //private void Start()
        //{
        //    Camera.main.GetOrAddComponent<TransformEx>().onChanged += OnCameraMove;
        //}
//#endif

        protected override void __OnDestroy()
        {
            if (Camera.main != null)
            {
                Camera.main.GetOrAddComponent<TransformEx>().onChanged -= OnCameraMove;
            }
            base.__OnDestroy();
        }
        private float m_cameraStopTime;
        public float updateIntervalTime { get; set; } = 2;
        public bool CanUpdatePaged { get => m_cameraStopTime > updateIntervalTime; }
        public float m_unloadTime;
        private void Update()
        {
            m_cameraStopTime += Time.deltaTime;
            m_unloadTime+= Time.deltaTime;
            if (m_unloadTime>60) {

                Resources.UnloadUnusedAssets();
                m_unloadTime = 0;
            }
        }

        public GameObject LoadOSGB(string rootPath, IEnumerable<string> filePath)
        {
            GameObject parent = new GameObject(rootPath);
            parent.transform.rotation = Quaternion.Euler(-90, 0, 0);
            StartCoroutine(coroutine_Load(rootPath, filePath,parent));
            return parent;
        }
        IEnumerator coroutine_Load(string rootPath, IEnumerable<string> filePath,GameObject parent)
        { 
            foreach (var item in filePath)
            {
                string fullFilePath = Path.Combine(rootPath, item);
                var op = osg_Reader.CreateFromWebRequest(fullFilePath);
                yield return op;
                if (op.reader != null)
                {
                    osgHelper.CreateGameObject(op.reader, parent);
                }
                else
                {
                    Debug.Log(fullFilePath + " read error");
                } 
            }  
            yield break;

        }
    }
}
