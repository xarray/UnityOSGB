using System; 
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine; 

namespace osgEx
{ 
    public class osgManager : MonoSingleton<osgManager>
    {
        private int frameCount;
        private bool needUpdateCalculateFrustumPlanes;
        private Plane[] m_calculateFrustumPlanes; 
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
        private Material m_material;
        public Material material
        {
            get
            {
                if (m_material == null)
                {
                    if (QualitySettings.renderPipeline == null)
                    {
                        m_material = new Material(Shader.Find("Standard"));
                    }
                    else
                    {
                        m_material = new Material(Shader.Find("Lit"));
                    }
                }
                return m_material;
            }
        }
        private int? m_materialMainTexID;
        public int materialMainTexID
        {
            get
            {
                m_materialMainTexID ??= material.GetTexturePropertyNameIDs()[0];
                return m_materialMainTexID.Value;
            }
        }
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
#if UNITY_EDITOR
        public string __rootPath;
        public string[] __filePath;
        private void Start()
        {
            Camera.main.GetOrAddComponent<TransformEx>().onChanged += OnCameraMove;
            if (string.IsNullOrEmpty(__rootPath) || __filePath == null)
            {
                return;
            }
            var dir = new DirectoryInfo(__rootPath);
            var dirs = dir.GetDirectories().Select(x => x.Name + "/" + x.Name + ".osgb").ToArray();

            LoadOSGB(__rootPath, dirs);
        }
#else
        private void Start()
        {
            Camera.main.GetOrAddComponent<TransformEx>().onChanged += OnCameraMove;
        }
#endif

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
            m_unloadTime += Time.deltaTime;
            if (m_unloadTime > 60)
            { 
                Resources.UnloadUnusedAssets();
                m_unloadTime = 0;
            }
        }

        public GameObject LoadOSGB(string rootPath, IEnumerable<string> filePath)
        {
            GameObject parent = new GameObject(rootPath);
            parent.transform.rotation = Quaternion.Euler(-90, 0, 0);
            foreach (var item in filePath)
            {
                GameObject o = new GameObject();
                o.transform.parent = parent.transform;
                o.transform.localPosition = Vector3.zero;
                o.transform.localRotation = Quaternion.identity;
                o.transform.localScale = Vector3.one;
                var loadHelper = o.AddComponent<osgMono_LoadHelper>();
                loadHelper.filePath = Path.Combine(rootPath, item); ;
                loadHelper.Load();
            }
            return parent;
        }
    }
}
