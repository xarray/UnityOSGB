using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ManaVR
{
    public class GlobalOsgInterface : MonoBehaviour
    {
        private static void userLogFunctionCallback(string str, int level)
        {
            switch (level)
            {
                case 1: Debug.LogWarning(str); break;
                case 2: Debug.LogError(str); break;
                default: Debug.Log(str); break;
            }
        }

        public enum NodeType
        {
            InvalidNode = 0,
            GeometryNode = 1,
            GroupNode = 2,
            LodNode = 3,
            PagedLodNode = 4
        };

        public enum TextureFormat
        {
            InvalidTexture = 0, Luminance = 1, LuminanceAlpha = 2, RGB = 3, RGBA = 4,
            Dxt1 = 13, Dxt1Alpha = 14, Dxt5 = 54
        };

        public enum TextureWrap
        {
            Clamp = 0, Repeat = 1, Mirror = 2
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UserLogFunctionDelegate(string str, int level);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr getPluginName();

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern int getPluginVersion();

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern void setUserLogFunction(System.IntPtr func);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern int requestNodeFile(System.IntPtr file);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr takeNewlyAddedNodes(int rootID, ref int count);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr takeNewlyRemovedNodes(int rootID, ref int count);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern bool beginReadNode(int id);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern bool beginReadPagedNode(int id, int location);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern bool endReadNode(bool eraseNodeData);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr updatePagedNodeState(int id, ref int stateCount);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern bool removeNode(int id);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readNodeLocalTransform(int subID, ref int count);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern bool readNodeLocalTransformDecomposition(int subID, System.IntPtr pos,
                                                                      System.IntPtr quat, System.IntPtr scale);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readNodeNameAndType(int subID, ref NodeType type);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern int  readNodeChildrenCount(int subID);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readNodeChildren(int subID, ref int count);
        
#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern int readNodeGlobalID(int subID);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern int readMeshCount(int subID);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readMeshes(int subID, ref int count);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readMeshWorldTransform(int subID, ref int count);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern bool readPrimitiveCounts(int subID, ref int numPts, ref int numLines, ref int numTri);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readPoints(int subID, ref int count);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readLines(int subID, ref int count);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readTriangles(int subID, ref int count);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern int readVertexCount(int subID);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readVertices(int subID, ref int count);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readVertexColors(int subID, ref int count);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readVertexColorsUB(int subID, ref int count);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readNormals(int subID, ref int count);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readUV(int subID, int channel, ref int count);
        
#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readTextureName(int subID);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern bool readTextureSize(int subID, ref int w, ref int h);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern TextureFormat readTextureFormat(int subID, ref TextureWrap wrapMode);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr readTexture(int subID, ref int dataSize);
        
#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern void setEyePosition(float x, float y, float z);
        
#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern void setViewTarget(float x, float y, float z);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern void setCameraUpDirection(float x, float y, float z);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern void setCameraProperties(float width, float height, float vFov,
                                                      float zNear, float zFar, float lodScale);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern bool updateDatabasePager(float deltaTime);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern void closeAll();

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern int beginIntersectWithLineSegment(
                System.IntPtr start, System.IntPtr end, float pointChkBias, bool pointsOnly, bool readAll);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern int beginIntersectWithPolytope(
                System.IntPtr normals, System.IntPtr distances, int planeCount, bool pointsOnly, bool readAll);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr getIntersectedPositions(ref int resultCount);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern System.IntPtr getIntersectedColors(ref int resultCount);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	    [DllImport ("__Internal")]
#else
        [DllImport("Mana_OpenSceneGraphLoader", CallingConvention = CallingConvention.Cdecl)]
#endif
        public static extern void endIntersection();

#if UNITY_WEBGL && !UNITY_EDITOR
	    [DllImport ("__Internal")]
	    private static extern void RegisterPlugin();
#endif

        public static Quaternion sceneRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        public float lodScale = 1.0f;
        public Camera mainCamera;

        void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
		    RegisterPlugin();
#endif
            
            string pluginName = Marshal.PtrToStringAnsi(getPluginName());
            int majorVersion = getPluginVersion() / 100, minorVersion = getPluginVersion() % 100;
            Debug.Log(pluginName + " Plugin v" + majorVersion + "." + minorVersion + " Loaded.");

            UserLogFunctionDelegate userLogFuncDelegate = new UserLogFunctionDelegate(userLogFunctionCallback);
            setUserLogFunction(Marshal.GetFunctionPointerForDelegate(userLogFuncDelegate));
        }

        IEnumerator Start()
        {
            yield return StartCoroutine("CallPluginAtEndOfFrames");
        }

        void Update()
        {
            Quaternion camRotation = Quaternion.Inverse(sceneRotation);
            if (mainCamera == null) mainCamera = Camera.main;

            Vector3 eye = camRotation * mainCamera.transform.position;
            Vector3 upPoint = camRotation * (mainCamera.transform.position + mainCamera.transform.up);
            Vector3 target = camRotation * (mainCamera.transform.position + mainCamera.transform.forward);
            setEyePosition(eye.x, eye.y, -eye.z);
            setViewTarget(target.x, target.y, -target.z);

            Vector3 up = (upPoint - eye).normalized;
            setCameraUpDirection(up.x, up.y, -up.z);
            setCameraProperties(mainCamera.pixelWidth, mainCamera.pixelHeight, mainCamera.fieldOfView,
                                mainCamera.nearClipPlane, mainCamera.farClipPlane, lodScale);
        }

        void OnApplicationQuit()
        {
            closeAll();
        }

        private IEnumerator CallPluginAtEndOfFrames()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();  // Wait until all frame rendering is done
                updateDatabasePager(Time.deltaTime);
            }
        }
        
        public static bool IntersectWithLineSegment(Vector3 start, Vector3 end, out Vector3 result, float bias = 0.01f,
                                                    bool onlyPoints = false, bool readAll = false)
        {
            Quaternion camRotation = Quaternion.Inverse(sceneRotation);
            Vector3 localS = camRotation * start, localE = camRotation * end;
            float[] s = new float[3] { localS.x, localS.y, -localS.z };
            float[] e = new float[3] { localE.x, localE.y, -localE.z };
            
            System.IntPtr ptrStart = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * 3);
            System.IntPtr ptrEnd = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * 3);
            Marshal.Copy(s, 0, ptrStart, 3);
            Marshal.Copy(e, 0, ptrEnd, 3);

            int numResults = beginIntersectWithLineSegment(ptrStart, ptrEnd, bias, onlyPoints, readAll);
            Marshal.FreeHGlobal(ptrStart);
            Marshal.FreeHGlobal(ptrEnd);

            if (numResults > 0)
            {
                System.IntPtr ptrResult = getIntersectedPositions(ref numResults);
                Marshal.Copy(ptrResult, s, 0, 3);
                result = sceneRotation * new Vector3(s[0], s[1], -s[2]);
                endIntersection();
                return true;
            }
            else result = new Vector3();
            return false;
        }
        
        public static Tuple<List<Vector3>, List<Color>> IntersectWithPolytope(
            List<Plane> inputPlanes, bool onlyPoints = false, bool readAll = false)
        {
            float[] normals = new float[inputPlanes.Count * 3];
            float[] distances = new float[inputPlanes.Count];
            Quaternion camRotation = Quaternion.Inverse(sceneRotation);
            for (int i = 0; i < inputPlanes.Count; ++i)
            {
                int index = 3 * i;
                Vector3 n = camRotation * inputPlanes[i].normal;
                normals[index + 0] = n.x;
                normals[index + 1] = n.y;
                normals[index + 2] = -n.z;
                distances[i] = inputPlanes[i].distance;
            }

            System.IntPtr ptrNormals = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * normals.Length);
            System.IntPtr ptrDistances = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * distances.Length);
            Marshal.Copy(normals, 0, ptrNormals, normals.Length);
            Marshal.Copy(distances, 0, ptrDistances, distances.Length);
            
            int numColors = 0, numResults = beginIntersectWithPolytope(
                ptrNormals, ptrDistances, inputPlanes.Count, onlyPoints, readAll);
            Marshal.FreeHGlobal(ptrNormals);
            Marshal.FreeHGlobal(ptrDistances);

            List<Vector3> resultPoints = new List<Vector3>();
            List<Color> resultColors = new List<Color>();
            if (numResults > 0)
            {
                System.IntPtr ptrResult = getIntersectedPositions(ref numResults);
                System.IntPtr ptrColors = getIntersectedColors(ref numColors);
                float[] results = new float[numResults * 3];
                float[] colors = new float[numColors * 4];
                Marshal.Copy(ptrResult, results, 0, numResults * 3);
                Marshal.Copy(ptrColors, colors, 0, numColors * 4);
                endIntersection();

                for (int i = 0; i < numResults; ++i)
                {
                    int index = 3 * i, index2 = 4 * i;
                    resultPoints.Add(sceneRotation * new Vector3(
                        results[index], results[index + 1], -results[index + 2]));
                    resultColors.Add(new Color(colors[index2 + 0], colors[index2 + 1],
                                               colors[index2 + 2], colors[index2 + 3]));
                }
            }
            return Tuple.Create<List<Vector3>, List<Color>>(resultPoints, resultColors);
        }
    }
}
