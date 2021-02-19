using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ReaderOSGB : MonoBehaviour
{
    const int OSG_HEADER_L = 0x6C910EA1;
    const int OSG_HEADER_H = 0x1AFB4545;

    public Material _template;
    public string _fileOrPathName;
    public bool _asOsgbFolder;

    public int _sceneType = 0, _version = 0;
    public bool _useBrackets = false;
    public bool _useSchemaData = false;
    public bool _useDomains = false;

    public string _currentFileName;
    public Plane[] _currentFrustum;
    public Texture2D _preloadedTexture = null;
    public Dictionary<uint, Object> _sharedObjects = new Dictionary<uint, Object>();
    public Dictionary<uint, Texture2D> _sharedTextures = new Dictionary<uint, Texture2D>();

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
        Debug.Log("OSGB file " + fileName  + ": version " + _version +
                  ", " + attributes.ToString("X"));

        _useBrackets = (attributes & 0x4) != 0;
        _useSchemaData = (attributes & 0x2) != 0;
        _useDomains = (attributes & 0x1) != 0;
        // TODO: handle attributes

        string compressor = ObjectBase.ReadString(reader);
        if (compressor != "0")
        {
            Debug.LogWarning("Decompressor " + compressor  + " not implemented");
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
        return LoadSceneData(fileName, new BinaryReader(stream));
    }
    
    void Start()
    {
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
    }

    void Update()
    {
        _currentFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
    }
}
