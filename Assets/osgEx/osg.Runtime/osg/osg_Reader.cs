﻿using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.Networking;

namespace osgEx
{

    public class osg_Reader
    {
        public class osg_AsyncOperation : CustomYieldInstruction
        {
            public override bool keepWaiting => !isDone;
            public bool isDone { get; private set; }
            //完成时返回的读取器
            public osg_Reader osgReader;
            public osg_AsyncOperation(string url)
            {
                //url=UnityWebRequest.EscapeURL(url);
                UnityWebRequest webRequest = UnityWebRequest.Get(url);
                webRequest.SendWebRequest().completed += (op) =>
                {
                    switch (webRequest.result)
                    {
                        case UnityWebRequest.Result.Success:
                            byte[] binary = webRequest.downloadHandler.data;
                            using (MemoryStream binartStream = new MemoryStream(binary))
                            {
                                using (BinaryReader binaryReader = new BinaryReader(binartStream))
                                {
                                    try
                                    {
                                        osgReader = osg_Reader.LoadFromBinaryReader(binaryReader, url);
                                        osgReader.filePath = url;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        Debug.Log(url);
                                        throw ex;
                                    }
                                }
                            }
                            break;
                        case UnityWebRequest.Result.InProgress:
                        case UnityWebRequest.Result.ConnectionError:
                        case UnityWebRequest.Result.ProtocolError:
                        case UnityWebRequest.Result.DataProcessingError:
                            Debug.Log(webRequest.result.ToString() + "\n" + url);
                            break;
                        default:
                            break;
                    }
                    isDone = true;
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

        public osg_Node root;

        public Dictionary<uint, osg_Object> _sharedObjects = new Dictionary<uint, osg_Object>();
        public Dictionary<uint, Texture2D> _sharedTextures = new Dictionary<uint, Texture2D>();

        static osg_Reader LoadFromBinaryReader(BinaryReader reader, string filePath)
        {
            var osg_reader = new osg_Reader();
            osg_reader.filePath = filePath;
            int magicNumL = reader.ReadInt32();
            int magicNumH = reader.ReadInt32();
            if (magicNumL != OSG_HEADER_L || magicNumH != OSG_HEADER_H)
            {
                Debug.LogWarning("Unmatched magic number");
            }
            osg_reader._sceneType = reader.ReadInt32();
            osg_reader._version = reader.ReadInt32();
            int attributes = reader.ReadInt32();
            //Debug.Log("OSGB file " + osg_reader.filePath + ": version " + osg_reader._version +   ", " + attributes.ToString("X")); 
            osg_reader._useBrackets = (attributes & 0x4) != 0;
            osg_reader._useSchemaData = (attributes & 0x2) != 0;
            osg_reader._useDomains = (attributes & 0x1) != 0;
            // TODO: handle attributes 
            string compressor = osg_Object.ReadString(reader);
            if (compressor != "0")
            {
                Debug.LogWarning("Decompressor " + compressor + " not implemented");
            }
            osg_reader.root = osg_Object.LoadObject(reader, osg_reader) as osg_Node;
            return osg_reader;
        }
        public static osg_Reader LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning("Unable to find file " + filePath);
                return null;
            }
            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                if (!stream.CanRead)
                {
                    Debug.LogWarning("Unable to read binary stream from " + filePath);
                    return null;
                }
                return LoadFromStream(stream, filePath);
            }
        }
        public static osg_AsyncOperation LoadFromWebRequest(string url)
        {
            return new osg_AsyncOperation(url);
        }
        public static osg_Reader LoadFromMemory(byte[] binary, string filePath)
        {
            using (MemoryStream binartStream = new MemoryStream(binary))
            {
                try
                {
                    return LoadFromStream(binartStream, filePath);
                }
                catch  
                {
                    Debug.Log(filePath);
                    throw;
                }
              
            }
        }
        public static osg_Reader LoadFromStream(Stream stream, string filePath)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                var osgReader = LoadFromBinaryReader(reader, filePath);
                return osgReader;
            }
        }
    }
}