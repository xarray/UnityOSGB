using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using UnityEngine;

namespace osgEx
{
    public class osg_Reader
    {
        public class osg_AsyncOperation : CustomYieldInstruction
        {
            UnityWebRequestAsyncOperation asyncOperation;
            public override bool keepWaiting => !asyncOperation.isDone;
            public bool isDone => asyncOperation.isDone;
            //完成时返回的读取器
            public osg_Reader reader;
            public osg_AsyncOperation(UnityWebRequestAsyncOperation asyncOperation, string url)
            {
                this.asyncOperation = asyncOperation;
                asyncOperation.completed += (op) =>
                {
                    var osg_reader = new osg_Reader();
                    osg_reader.filePath = url;
                    switch (asyncOperation.webRequest.result)
                    {
                        case UnityWebRequest.Result.Success:
                            byte[] binary = asyncOperation.webRequest.downloadHandler.data;
                            using (MemoryStream stream = new MemoryStream(binary))
                            {
                                using (BinaryReader reader = new BinaryReader(stream))
                                {

                                    int magicNumL = reader.ReadInt32();
                                    int magicNumH = reader.ReadInt32();
                                    if (magicNumL != OSG_HEADER_L || magicNumH != OSG_HEADER_H)
                                    {
                                        Debug.LogWarning("Unmatched magic number");
                                        break;
                                    }
                                    osg_reader._sceneType = reader.ReadInt32();
                                    osg_reader._version = reader.ReadInt32();
                                    int attributes = reader.ReadInt32();
                                    //Debug.Log("OSGB file " + fileName + ": version " + _version +
                                    //           ", " + attributes.ToString("X")); 
                                    osg_reader._useBrackets = (attributes & 0x4) != 0;
                                    osg_reader._useSchemaData = (attributes & 0x2) != 0;
                                    osg_reader._useDomains = (attributes & 0x1) != 0;
                                    // TODO: handle attributes 
                                    string compressor = osg_Object.ReadString(reader);
                                    if (compressor != "0")
                                    {
                                        Debug.LogWarning("Decompressor " + compressor + " not implemented");
                                    }
                                    var value = osg_Object.LoadObject(reader, osg_reader);
                                    osg_reader.mainNode = value as osg_Node;
                                    this.reader = osg_reader;

                                }
                            }
                            break;
                        case UnityWebRequest.Result.InProgress:
                            break;
                        case UnityWebRequest.Result.ConnectionError:
                        case UnityWebRequest.Result.ProtocolError:
                        case UnityWebRequest.Result.DataProcessingError:
                            Debug.Log(asyncOperation.webRequest.result.ToString() + "\n" + asyncOperation.webRequest.url);
                            break;
                        default:
                            break;
                    }


                };
            }
        }
        private osg_Reader() { }

        const int OSG_HEADER_L = 0x6C910EA1;
        const int OSG_HEADER_H = 0x1AFB4545;
        public int _sceneType = 0, _version = 0;
        public bool _useBrackets = false;
        public bool _useSchemaData = false;
        public bool _useDomains = false;

        public string filePath;

        public osg_Node mainNode;

        public Dictionary<uint, osg_Object> _sharedObjects = new Dictionary<uint, osg_Object>();
        public Dictionary<uint, Texture2D> _sharedTextures = new Dictionary<uint, Texture2D>();

        public static osg_Reader CreateFromIO(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Debug.LogWarning("Unable to find file " + fileName);
                return null;
            }
            using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                if (!stream.CanRead)
                {
                    Debug.LogWarning("Unable to read binary stream from " + fileName);
                    return null;
                }
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    osg_Reader current = new osg_Reader();
                    current.filePath = fileName;
                    int magicNumL = reader.ReadInt32();
                    int magicNumH = reader.ReadInt32();
                    if (magicNumL != OSG_HEADER_L || magicNumH != OSG_HEADER_H)
                    {
                        Debug.LogWarning("Unmatched magic number");
                        return null;
                    }
                    current._sceneType = reader.ReadInt32();
                    current._version = reader.ReadInt32();
                    int attributes = reader.ReadInt32();
                    //Debug.Log("OSGB file " + fileName + ": version " + _version +
                    //           ", " + attributes.ToString("X"));

                    current._useBrackets = (attributes & 0x4) != 0;
                    current._useSchemaData = (attributes & 0x2) != 0;
                    current._useDomains = (attributes & 0x1) != 0;
                    // TODO: handle attributes

                    string compressor = osg_Object.ReadString(reader);
                    if (compressor != "0")
                    {
                        Debug.LogWarning("Decompressor " + compressor + " not implemented");
                        return null;
                    }
                    current.mainNode = osg_Object.LoadObject(reader, current) as osg_Node;
                    current.filePath = fileName;
                    return current;
                }
            }
        }

        public static osg_AsyncOperation CreateFromWebRequest(string url)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            return new osg_AsyncOperation(webRequest.SendWebRequest(), url);
        }
    }
}
