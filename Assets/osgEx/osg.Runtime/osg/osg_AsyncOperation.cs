using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace osgEx
{
    public class osg_AsyncOperation : CustomYieldInstruction
    {
        public override bool keepWaiting => !isDone;
        public bool isDone { get; private set; }
        //完成时返回的读取器
        public osg_Reader osgReader;
        private string url;
        public osg_AsyncOperation(string url)
        {
            this.url = url;
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            webRequest.SendWebRequest().completed += onWebRequestCompleted;
        }
        void onWebRequestCompleted(AsyncOperation asyncOperation)
        {
            UnityWebRequestAsyncOperation requestAsyncOperation = (UnityWebRequestAsyncOperation)asyncOperation;
            byte[] binary = requestAsyncOperation.webRequest.downloadHandler.data;
            switch (requestAsyncOperation.webRequest.result)
            {
                case UnityWebRequest.Result.Success:
#if UNITY_EDITOR || !UNITY_WEBGL
                    Task.Run(() => LoadFromBinary(binary)).GetAwaiter().OnCompleted(() => { isDone = true; });
#else
                    LoadFromBinary(binary);
                    isDone = true;
#endif

                    break;
                case UnityWebRequest.Result.InProgress:
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.Log(requestAsyncOperation.webRequest.result.ToString() + "\n" + url);
                    isDone = true;
                    break;
                default:
                    break;
            }

        }
        void LoadFromBinary(byte[] binary)
        {
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
                        Debug.Log(url + "\n\r" + ex.Message);
                        throw ex;
                    }
                }
            }
        }
    }
}
