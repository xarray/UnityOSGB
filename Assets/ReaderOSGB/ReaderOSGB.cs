using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class ReaderOSGB : MonoBehaviour
    {
        const int OSG_HEADER_L = 0x6C910EA1;
        const int OSG_HEADER_H = 0x1AFB4545;

        public Material _template;
        public string _fileOrPathName;
        public bool _asOsgbFolder, _withMeshCollider;

        public int _sceneType = 0, _version = 0;
        public bool _useBrackets = false;
        public bool _useSchemaData = false;
        public bool _useDomains = false;

        public string _currentFileName;
        public Plane[] _currentFrustum;
        public Texture2D _preloadedTexture = null;
        public Dictionary<uint, Object> _sharedObjects = new Dictionary<uint, Object>();
        public Dictionary<uint, Texture2D> _sharedTextures = new Dictionary<uint, Texture2D>();

        public class PagingRequest
        {
            public PagedData pagedData;
            public int childIndex, updatedStamp;
        }
        List<PagingRequest> _loadingRequests = new List<PagingRequest>();
        List<PagingRequest> _unloadingRequests = new List<PagingRequest>();
        Task _pagingRequestHandler;
        object _taskMutex = new object();
        bool _isTaskRunning = false;

        public void RequestLoadingAndUnloading(PagedData data, List<int> toLoad, List<int> toUnload)
        {
            lock (_taskMutex)
            {
                foreach (int index in toLoad)
                {
                    PagingRequest req = _loadingRequests.Find(
                        c => c.pagedData.Equals(data) && c.childIndex.Equals(index));
                    if (req == null)
                    {
                        req = new PagingRequest
                        { pagedData = data, childIndex = index, updatedStamp = Time.frameCount };
                        _loadingRequests.Add(req);
                    }
                    else
                        req.updatedStamp = Time.frameCount;
                }

                foreach (int index in toUnload)
                {
                    PagingRequest req = _unloadingRequests.Find(
                        c => c.pagedData.Equals(data) && c.childIndex.Equals(index));
                    if (req == null)
                    {
                        req = new PagingRequest
                        { pagedData = data, childIndex = index, updatedStamp = Time.frameCount };
                        _unloadingRequests.Add(req);
                    }
                    else
                        req.updatedStamp = Time.frameCount;
                }
            }
        }

        void LoadOrUnloadData(PagedData data, int index, bool toLoad)
        {
            if (toLoad)
            {
                string fileName = data.getFullFileName(index);
                GameObject fineNode = LoadSceneFromFile(fileName);
                if (fineNode != null)
                {
                    fineNode.name = Path.GetFileNameWithoutExtension(fileName);
                    fineNode.transform.SetParent(data.gameObject.transform, false);
                    data._pagedNodes[index] = fineNode;
                    data._pagedNodes[0].SetActive(false);  // FIXME: assume only 1 rough level
                }
                else
                    Debug.LogWarning("Unable to read OSGB data from " + fileName);
            }
            else
            {
                Destroy(data._pagedNodes[index]);
                data._pagedNodes[index] = null;
                data._pagedNodes[0].SetActive(true);  // FIXME: assume only 1 rough level
            }
        }

        IEnumerator PagingTask()
        {
            _isTaskRunning = true;
            while (_isTaskRunning)
            {
                PagedData pData0 = null;
                int index0 = -1, currentFrame = Time.frameCount;
                lock (_taskMutex)
                {
                    foreach (PagingRequest req in _loadingRequests)
                    {
                        if (req.updatedStamp >= currentFrame - 1)  // latest req
                        {
                            pData0 = req.pagedData; index0 = req.childIndex;
                            _loadingRequests.Remove(req); break;
                        }
                    }

                    if (index0 < 0)
                    {
                        for (int i = 0; i < _loadingRequests.Count;)
                        {
                            PagingRequest req = _loadingRequests[i];
                            if (req.updatedStamp < currentFrame - 10)  // too-old req
                                _loadingRequests.RemoveAt(i);
                            else ++i;
                        }
                    }

                    if (_unloadingRequests.Count > 60)
                    {
                        for (int i = 0; i < _unloadingRequests.Count;)
                        {
                            PagingRequest req = _unloadingRequests[i];
                            if (req.updatedStamp >= currentFrame - 1)  // latest req
                            {
                                LoadOrUnloadData(req.pagedData, req.childIndex, false);
                                _unloadingRequests.RemoveAt(i);
                            }
                            else if (req.updatedStamp < currentFrame - 10)  // too-old req
                                _unloadingRequests.RemoveAt(i);
                            else ++i;
                        }
                    }
                }

                if (pData0 && index0 >= 0)
                    LoadOrUnloadData(pData0, index0, true);
                //Debug.Log(_loadingRequests.Count + ", " + _unloadingRequests.Count);
                yield return null;
            }
        }

        IEnumerator StopPagingTask(Task task)
        {
            _isTaskRunning = false;
            yield return null;
            task.Stop();
        }

        public GameObject LoadSceneData(string fileName, BinaryReader reader)
        {
            // Load header data
            int magicNumL = reader.ReadInt32();
            int magicNumH = reader.ReadInt32();
            if (magicNumL != OSG_HEADER_L || magicNumH != OSG_HEADER_H)
            {
                Debug.LogWarning("Unmatched magic number");
                return null;
            }

            _sceneType = reader.ReadInt32();
            _version = reader.ReadInt32();
            int attributes = reader.ReadInt32();
            Debug.Log("OSGB file " + fileName + ": version " + _version +
                      ", " + attributes.ToString("X"));

            _useBrackets = (attributes & 0x4) != 0;
            _useSchemaData = (attributes & 0x2) != 0;
            _useDomains = (attributes & 0x1) != 0;
            // TODO: handle attributes

            string compressor = ObjectBase.ReadString(reader);
            if (compressor != "0")
            {
                Debug.LogWarning("Decompressor " + compressor + " not implemented");
                return null;
            }

            // Load root object
            GameObject scene = new GameObject(Path.GetFileNameWithoutExtension(fileName));
            if (!ObjectBase.LoadObject(scene, reader, this))
            {
                Debug.LogWarning("Failed to load scene");
                return null;
            }

            // Clear temperatory variables
            _preloadedTexture = null;
            _sharedObjects.Clear();
            _sharedTextures.Clear();
            return scene;
        }

        public GameObject LoadSceneFromFile(string fileName)
        {
            _currentFileName = fileName;
            if (!File.Exists(fileName))
            {
                Debug.LogWarning("Unable to find file " + fileName);
                return null;
            }

            FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
            if (!stream.CanRead)
            {
                Debug.LogWarning("Unable to read binary stream from " + fileName);
                return null;
            }

            GameObject gameScene = LoadSceneData(fileName, new BinaryReader(stream));
            stream.Close(); return gameScene;
        }

        void Start()
        {
            _pagingRequestHandler = new Task(PagingTask());
            if (_template == null)
                _template = new Material(Shader.Find("Standard"));

            if (_asOsgbFolder)
            {
                foreach (string folderName in Directory.GetDirectories(_fileOrPathName))
                {
                    string rootFile = folderName + Path.DirectorySeparatorChar
                                    + Path.GetFileName(folderName) + ".osgb";
                    GameObject scene = LoadSceneFromFile(rootFile);
                    if (scene != null)
                        scene.transform.SetParent(this.transform, false);
                    else
                        Debug.LogWarning("Unable to read OSGB data from " + rootFile);
                }
            }
            else
            {
                GameObject scene = LoadSceneFromFile(_fileOrPathName);
                if (scene != null)
                    scene.transform.SetParent(this.transform, false);
                else
                    Debug.LogWarning("Unable to read OSGB data from " + _fileOrPathName);
            }

            // Get global center & extents
            MeshFilter[] mfList = GetComponentsInChildren<MeshFilter>();
            Bounds totalBounds = new Bounds();
            for (int j = 0; j < mfList.Length; ++j)
            {
                Matrix4x4 l2w = mfList[j].transform.localToWorldMatrix;
                Vector3[] vertices = mfList[j].sharedMesh.vertices;
                Bounds meshBounds = new Bounds();
                for (int i = 0; i < vertices.Length; ++i)
                {
                    if (i == 0) meshBounds.center = l2w.MultiplyPoint(vertices[i]);
                    else meshBounds.Encapsulate(l2w.MultiplyPoint(vertices[i]));
                }

                if (j == 0) totalBounds = meshBounds;
                else totalBounds.Encapsulate(meshBounds);
            }
            this.transform.position = -totalBounds.center;
        }

        void Update()
        {
            _currentFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        }

        void OnDestroy()
        {
            StopPagingTask(_pagingRequestHandler);
        }
    }
}
