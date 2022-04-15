using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class PagedData : MonoBehaviour
    {
        public enum RangeMode { Distance, PixelSize };
        public RangeMode _rangeMode = RangeMode.Distance;
        public string _rootFileName = "", _databasePath = "";
        public BoundingSphere _bounds;
        public ReaderOSGB _mainReader;

        public List<string> _fileNames = new List<string>();
        public List<Vector2> _ranges = new List<Vector2>();
        public List<GameObject> _pagedNodes = new List<GameObject>();
        string _fullPathPrefix = "";

        public string getFullFileName(int index)
        { return _fullPathPrefix + _fileNames[index]; }

        void Start()
        {
            _fullPathPrefix = Path.GetDirectoryName(_rootFileName) + Path.DirectorySeparatorChar;
            if (_databasePath.Length > 0) _fullPathPrefix += _databasePath + Path.DirectorySeparatorChar;
            while (_pagedNodes.Count < _fileNames.Count) _pagedNodes.Add(null);
        }

        void Update()
        {
            Camera mainCam = Camera.main;
            Matrix4x4 world2LocalOwner = _mainReader.transform.worldToLocalMatrix;
            Matrix4x4 local2World = this.transform.localToWorldMatrix * world2LocalOwner;
            Vector3 cameraPos = world2LocalOwner.MultiplyPoint(mainCam.transform.position);

            // Check LOD situation
            float rangeValue = 0.0f;
            if (_rangeMode == RangeMode.Distance)
            {
                Vector3 centerW = local2World.MultiplyPoint(_bounds.position);
                rangeValue = (cameraPos - centerW).magnitude;
            }
            else
            {
                Vector3 centerW = local2World.MultiplyPoint(_bounds.position);
                Bounds bb = new Bounds(
                    centerW, new Vector3(_bounds.radius, _bounds.radius, _bounds.radius) * 2.0f);
                if (GeometryUtility.TestPlanesAABB(_mainReader._currentFrustum, bb))
                {
                    float distance = (centerW - cameraPos).magnitude;
                    float slope = Mathf.Tan(mainCam.fieldOfView * Mathf.Deg2Rad * 0.5f);
                    float projFactor = (0.5f * mainCam.pixelHeight) / (slope * distance);
                    rangeValue = _bounds.radius * projFactor;  // screenPixelRadius
                }
                else
                    rangeValue = -1.0f;
            }

            // Find files to load/unload
            List<int> filesToLoad = new List<int>();
            List<int> filesToUnload = new List<int>();
            for (int i = 0; i < _ranges.Count; ++i)
            {
                string fileName = _fileNames[i];
                if (fileName.Length == 0) continue;

                Vector2 range = _ranges[i];
                bool unloaded = (_pagedNodes[i] == null);
                if (range[0] < rangeValue && rangeValue < range[1])
                { if (unloaded) filesToLoad.Add(i); }
                else if (!unloaded) filesToUnload.Add(i);
            }

            // Update file loading/unloading status
            _mainReader.RequestLoadingAndUnloading(this, filesToLoad, filesToUnload);
        }
    }
}
