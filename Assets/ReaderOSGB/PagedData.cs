using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
    float _scaleBase = (Mathf.PI * 0.1f) / (1920.0f * 1080.0f);

    void Start()
    {
        _fullPathPrefix = Path.GetDirectoryName(_rootFileName) + Path.DirectorySeparatorChar;
        if (_databasePath.Length > 0) _fullPathPrefix += _databasePath + Path.DirectorySeparatorChar;
        while (_pagedNodes.Count < _fileNames.Count) _pagedNodes.Add(null);
    }

    void Update()
    {
        Camera mainCam = Camera.main;
        Matrix4x4 world2Local = this.transform.worldToLocalMatrix;
        Matrix4x4 local2World = this.transform.localToWorldMatrix;

        // Check LOD situation
        float rangeValue = 0.0f;
        if (_rangeMode == RangeMode.Distance)
        {
            Vector3 centerW = local2World.MultiplyPoint(_bounds.position);
            rangeValue = (mainCam.transform.position - centerW).magnitude;
        }
        else
        {
            Vector3 centerW = local2World.MultiplyPoint(_bounds.position);
            Bounds bb = new Bounds(centerW, new Vector3(_bounds.radius, _bounds.radius, _bounds.radius) * 2.0f);
            if (GeometryUtility.TestPlanesAABB(_mainReader._currentFrustum, bb))
            {
                float distance = (centerW - mainCam.transform.position).magnitude;
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
            else if (!unloaded)
                filesToUnload.Add(i);
        }

        // Update file loading/unloading status
        foreach (int index in filesToLoad)
        {
            string fileName = _fullPathPrefix + _fileNames[index];
            GameObject fineNode = _mainReader.LoadSceneFromFile(fileName);
            if (fineNode != null)
            {
                fineNode.transform.SetParent(this.transform, false);
                _pagedNodes[index] = fineNode;
                _pagedNodes[0].SetActive(false);  // FIXME: assume only 1 rough level
            }
            else
                Debug.LogWarning("Unable to read OSGB data from " + fileName);
        }

        foreach (int index in filesToUnload)
        {
            Destroy(_pagedNodes[index]);
            _pagedNodes[index] = null;
            _pagedNodes[0].SetActive(true);  // FIXME: assume only 1 rough level
        }
    }
}
