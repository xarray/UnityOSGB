using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace ManaVR
{
    public class MeshLoader : MonoBehaviour
    {
        public Material standardMaterial;
        public string filename;
        public float colorMax = 1.0f;
        public bool eraseLowLevelGeometry = true;
        public bool useHierarchicalTransforms = true;

        private System.IntPtr posToCopy = System.IntPtr.Zero, quatToCopy = System.IntPtr.Zero,
                              scaleToCopy = System.IntPtr.Zero;
        private Dictionary<int, GameObject> pagedNodeMap = new Dictionary<int, GameObject>();
        private int rootNodeID = 0;

        /// <Test>
        GameObject selectedObj = null;

        void LateUpdate()
        {
            if (Input.GetMouseButtonUp(0))
            {
                Ray mouseLine = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 result, mouseStart = mouseLine.origin + mouseLine.direction * Camera.main.nearClipPlane;
                Vector3 mouseEnd = mouseLine.origin + mouseLine.direction * Camera.main.farClipPlane;

                if (GlobalOsgInterface.IntersectWithLineSegment(mouseStart, mouseEnd, out result, 0.01f, true))
                {
                    if (selectedObj == null)
                    {
                        selectedObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        selectedObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                    }
                    selectedObj.transform.position = result;
                    Debug.Log("Selected point " + result);
                }
                else
                    Debug.Log("No selection");
            }
            else if (Input.GetMouseButtonUp(1))
            {
                List<Plane> planes = new List<Plane>();
                planes.Add(new Plane(new Vector3(1.0f, 0.0f, 0.0f), new Vector3(-5.0f, 0.0f, 0.0f)));
                planes.Add(new Plane(new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(5.0f, 0.0f, 0.0f)));
                planes.Add(new Plane(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, -5.0f, 0.0f)));
                planes.Add(new Plane(new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, 5.0f, 0.0f)));
                planes.Add(new Plane(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, -5.0f)));
                planes.Add(new Plane(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 0.0f, 5.0f)));

                Tuple<List<Vector3>, List<Color>> results = GlobalOsgInterface.IntersectWithPolytope(planes, true, false);
                Debug.Log("Positions = " + results.Item1.Count + "; Colors = " + results.Item2.Count);
            }
        }
        /// </Test>

        void Start()
        {
            posToCopy = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * 3);
            quatToCopy = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * 4);
            scaleToCopy = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * 3);
            if (standardMaterial == null)
                standardMaterial = new Material(Shader.Find("Standard"));

            rootNodeID = GlobalOsgInterface.requestNodeFile(Marshal.StringToHGlobalAnsi(filename));
            if (rootNodeID > 0)
            {
                GameObject rootNodeObj = new GameObject("Root node");
                rootNodeObj.transform.SetParent(transform, false);
                transform.localRotation = GlobalOsgInterface.sceneRotation;

                GlobalOsgInterface.beginReadNode(rootNodeID);
                IterateNodeData(rootNodeObj, 0);
                GlobalOsgInterface.endReadNode(eraseLowLevelGeometry);  // Erase low-level gemetry and texture data
            }
        }

        void OnDestroy()
        {
            Marshal.FreeHGlobal(posToCopy);
            Marshal.FreeHGlobal(quatToCopy);
            Marshal.FreeHGlobal(scaleToCopy);
        }

        void Update()
        {
            // Find and add/remove paged node children
            int addedCount = 0, removedCount = 0;
            System.IntPtr sys = GlobalOsgInterface.takeNewlyAddedNodes(rootNodeID, ref addedCount);
            System.IntPtr sys2 = GlobalOsgInterface.takeNewlyRemovedNodes(rootNodeID, ref removedCount);
            GameObject parentObj = null;
            
            if (addedCount > 0)
            {
                int[] addedNodes = new int[addedCount * 2];
                Marshal.Copy(sys, addedNodes, 0, addedCount * 2);
                for (int i = 0; i < addedNodes.Length; i += 2)
                {
                    bool ok = GlobalOsgInterface.beginReadPagedNode(addedNodes[i], addedNodes[i + 1]);
                    if (ok)
                    {
                        GameObject childObj = new GameObject("PagedLevel_" + addedNodes[i + 1]);
                        if (pagedNodeMap.TryGetValue(addedNodes[i], out parentObj))
                            childObj.transform.SetParent(parentObj.transform, false);
                        IterateNodeData(childObj, 0);
                        GlobalOsgInterface.endReadNode(eraseLowLevelGeometry);
                        
                        PagedChildID pagedID = childObj.AddComponent<PagedChildID>();
                        pagedID.parentID = addedNodes[i];
                        pagedID.location = addedNodes[i + 1];
                    }
                    else
                        Debug.LogWarning("[MeshLoader] Failed to read paged node data " +
                                         addedNodes[i] + ":" + addedNodes[i + 1]);
                }
            }

            if (removedCount > 0)
            {
                int[] removedNodes = new int[removedCount * 2];
                Marshal.Copy(sys2, removedNodes, 0, removedCount * 2);
                for (int i = 0; i < removedNodes.Length; i += 2)
                {
                    if (pagedNodeMap.TryGetValue(removedNodes[i], out parentObj))
                    {
                        foreach (Transform child in parentObj.transform)
                        {
                            PagedChildID pagedID = child.GetComponent<PagedChildID>();
                            if (pagedID != null && pagedID.location == removedNodes[i + 1])
                            {
                                // Clear child pagedNodeMap
                                DestroyChildPagedNodes(child, ref pagedNodeMap);

                                // Remove me
                                MeshFilter[] filters = child.GetComponentsInChildren<MeshFilter>();
                                if (filters.Length > 0) foreach (MeshFilter mf in filters) Destroy(mf.sharedMesh);
                                Destroy(child.gameObject);
                            }
                        }
                    }
                    else
                        Debug.LogWarning("[MeshLoader] Failed to remove unregistered paged data: " +
                                         removedNodes[i] + ":" + removedNodes[i + 1]);
                }
            }

            // Traverse all paged nodes and update their LOD stats
            List<int> nodeToRemove = new List<int>();
            foreach (KeyValuePair<int, GameObject> entry in pagedNodeMap)
            {
                int locationCount = 0;
                GameObject pagedNode = entry.Value;
                System.IntPtr sysState = GlobalOsgInterface.updatePagedNodeState(entry.Key, ref locationCount);
                if (sysState == System.IntPtr.Zero || locationCount == 0) continue;
                else if (locationCount == -2) { nodeToRemove.Add(entry.Key); continue; }
                else if (locationCount == -1)
                {
                    Debug.LogWarning("Unable to get paged information from " + pagedNode.name);
                    continue;
                }

                int[] locArray = new int[locationCount];
                Marshal.Copy(sysState, locArray, 0, locationCount);
                List<int> locations = new List<int>(locArray);
                foreach (Transform child in pagedNode.transform)
                {
                    PagedChildID idObj = child.GetComponent<PagedChildID>();
                    if (idObj != null && locations.Contains(idObj.location)) child.gameObject.SetActive(true);
                    else child.gameObject.SetActive(false);
                }
            }
            foreach (int key in nodeToRemove)
                pagedNodeMap.Remove(key);
        }

        private void IterateNodeData(GameObject obj, int subID)
        {
            if (useHierarchicalTransforms)
            {
                float[] pos = new float[3], quat = new float[4], scale = new float[3];
                GlobalOsgInterface.readNodeLocalTransformDecomposition(subID, posToCopy, quatToCopy, scaleToCopy);
                Marshal.Copy(posToCopy, pos, 0, 3);
                Marshal.Copy(quatToCopy, quat, 0, 4);
                Marshal.Copy(scaleToCopy, scale, 0, 3);
                obj.transform.localPosition = new Vector3(pos[0], pos[1], -pos[2]);  // RH to LH
                obj.transform.localRotation = new Quaternion(quat[0], quat[1], -quat[2], -quat[3]);  // RH to LH
                obj.transform.localScale = new Vector3(scale[0], scale[1], scale[2]);
            }

            GlobalOsgInterface.NodeType type = GlobalOsgInterface.NodeType.InvalidNode;
            string nodeName = Marshal.PtrToStringAnsi(GlobalOsgInterface.readNodeNameAndType(subID, ref type));
            if (nodeName != null && nodeName.Length > 0) obj.name = nodeName;
            switch (type)
            {
                case GlobalOsgInterface.NodeType.GeometryNode:
                    {
                        int meshCount = 0;
                        System.IntPtr sys2 = GlobalOsgInterface.readMeshes(subID, ref meshCount);
                        int[] meshes = new int[meshCount];
                        Marshal.Copy(sys2, meshes, 0, meshCount);
                        if (meshCount == 0) break;

                        List<Material> materials = new List<Material>();
                        Dictionary<string, int> materialSharedMap = new Dictionary<string, int>();
                        CombineInstance[] meshDataList = new CombineInstance[meshCount];
                        for (int i = 0; i < meshCount; ++i)
                        {
                            // Create new mesh and find assoicated texture name
                            string texName = "";
                            meshDataList[i].mesh = CreateMesh(meshes[i], ref texName);

                            // Use existing texture index if meet the same filename
                            if (materialSharedMap.ContainsKey(texName))
                                meshDataList[i].subMeshIndex = materialSharedMap[texName];
                            else
                            {
                                // Create a new material for new texture
                                Material newMat = new Material(standardMaterial);
                                newMat.mainTexture = CreateTexture(meshes[i], texName);

                                // Add the new index to the shared map
                                meshDataList[i].subMeshIndex = materials.Count;
                                materialSharedMap[texName] = materials.Count;
                                materials.Add(newMat);
                            }
                        }
                        
#if false  // TODO: combining mesh is of no use here...
                        Mesh totalGeometry = (meshCount > 1) ? new Mesh() : meshDataList[0].mesh;
                        if (meshCount > 1)
                        {
                            totalGeometry.name = "Mesh_" + subID;
                            totalGeometry.CombineMeshes(meshDataList, true, false);
                        }
                        AttachMeshRenderer(obj, totalGeometry, ref materials);
#else
                        if (meshCount > 1)
                        {
                            foreach (CombineInstance ci in meshDataList)
                            {
                                GameObject meshObj = new GameObject("SubMesh_" + ci.subMeshIndex);
                                meshObj.transform.SetParent(obj.transform, false);
                                List<Material> subMaterials = new List<Material>();
                                subMaterials.Add(materials[ci.subMeshIndex]);
                                AttachMeshRenderer(meshObj, ci.mesh, ref subMaterials);
                            }
                        }
                        else if (meshCount == 1)
                            AttachMeshRenderer(obj, meshDataList[0].mesh, ref materials);
#endif
                    }
                    break;
                case GlobalOsgInterface.NodeType.GroupNode:
                case GlobalOsgInterface.NodeType.PagedLodNode:
                    {
                        int childrenCount = 0, globalID = GlobalOsgInterface.readNodeGlobalID(subID);
                        if (globalID > 0) pagedNodeMap[globalID] = obj;

                        System.IntPtr sys2 = GlobalOsgInterface.readNodeChildren(subID, ref childrenCount);
                        int[] children = new int[childrenCount];
                        Marshal.Copy(sys2, children, 0, childrenCount);

                        for (int i = 0; i < childrenCount; ++i)
                        {
                            GameObject childObj = new GameObject(
                                type == GlobalOsgInterface.NodeType.PagedLodNode ? "RoughLevel_" + i : "Node_" + i);
                            childObj.transform.SetParent(obj.transform, false);
                            IterateNodeData(childObj, children[i]);

                            if (type == GlobalOsgInterface.NodeType.PagedLodNode)
                            {
                                PagedChildID pagedID = childObj.AddComponent<PagedChildID>();
                                pagedID.parentID = globalID;
                                pagedID.location = i;
                            }
                        }
                    }
                    break;
                default:
                    Debug.LogWarning("[MeshLoader] Unsupported node type " + type);
                    break;
            }
        }

        private Mesh CreateMesh(int subID, ref string textureName)
        {
            // Read mesh transform if required
            Matrix4x4 matrix = new Matrix4x4();
            if (!useHierarchicalTransforms)
            {
                int count = 16;
                System.IntPtr sys = GlobalOsgInterface.readMeshWorldTransform(subID, ref count);
                matrix = CreateMatrixFromPtr(sys, count);
            }
            Vector3 posInMatrix = new Vector3(matrix.m30, matrix.m31, -matrix.m32);  // RH to LH

            // Read mesh vertices and vertex attributes
            int vertexCount = GlobalOsgInterface.readVertexCount(subID);
            int vCount = 0, nCount = 0, cCount = 0, tCount = 0;
            System.IntPtr sysV = GlobalOsgInterface.readVertices(subID, ref vCount);
            System.IntPtr sysN = GlobalOsgInterface.readNormals(subID, ref nCount);
            System.IntPtr sysT = GlobalOsgInterface.readUV(subID, 0, ref tCount);

            textureName = Marshal.PtrToStringAnsi(GlobalOsgInterface.readTextureName(subID));
            if (textureName == null) textureName = "";

            // Create mesh vertices and triangles
            Mesh mesh = new Mesh();
            mesh.name = "Mesh_" + subID;
            mesh.Clear();
            if (vCount == vertexCount * 3)
            {
                float[] va = new float[vCount]; Marshal.Copy(sysV, va, 0, vCount);
                Vector3[] vertices = new Vector3[vertexCount];
                for (int i = 0; i < vertexCount; ++i)
                {
                    Vector3 v = new Vector3(va[3 * i], va[3 * i + 1], -va[3 * i + 2]);  // RH to LH
                    if (useHierarchicalTransforms) vertices[i] = v;
                    else vertices[i] = matrix.MultiplyPoint3x4(v) + posInMatrix;
                }
                mesh.vertices = vertices;
            }

            bool hasNormals = true;
            if (nCount == vertexCount * 3)
            {
                float[] na = new float[nCount]; Marshal.Copy(sysN, na, 0, nCount);
                Vector3[] normals = new Vector3[vertexCount];
                for (int i = 0; i < vertexCount; ++i)
                    normals[i] = new Vector3(na[3 * i], na[3 * i + 1], na[3 * i + 2]);
                mesh.normals = normals;
            }
            else hasNormals = false;

            System.IntPtr sysC = GlobalOsgInterface.readVertexColors(subID, ref cCount);
            if (cCount == vertexCount * 4)
            {
                float[] ca = new float[cCount]; Marshal.Copy(sysC, ca, 0, cCount);
                float invRange = 1.0f / colorMax;

                Color[] colors = new Color[vertexCount];
                for (int i = 0; i < vertexCount; ++i)
                    colors[i] = new Color(ca[4 * i] * invRange, ca[4 * i + 1] * invRange,
                                          ca[4 * i + 2] * invRange, ca[4 * i + 3] * invRange);
                mesh.colors = colors;
            }

            if (tCount == vertexCount * 2)
            {
                float[] ta = new float[tCount]; Marshal.Copy(sysT, ta, 0, tCount);
                Vector2[] uv = new Vector2[vertexCount];
                for (int i = 0; i < vertexCount; ++i)
                    uv[i] = new Vector2(ta[2 * i], ta[2 * i + 1]);
                mesh.uv = uv;
            }

            // Read points/lines/triangles
            int pointCount = 0, lineCount = 0, triangleCount = 0;
            GlobalOsgInterface.readPrimitiveCounts(subID, ref pointCount, ref lineCount, ref triangleCount);
            mesh.indexFormat = mesh.vertices.Length > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;

            if (triangleCount > 0)
            {
                System.IntPtr sysTri = GlobalOsgInterface.readTriangles(subID, ref triangleCount);
                int[] triangles = new int[triangleCount];
                Marshal.Copy(sysTri, triangles, 0, triangleCount);
                mesh.triangles = triangles;
            }
            else if (pointCount > 0)
            {
                System.IntPtr sysPts = GlobalOsgInterface.readPoints(subID, ref pointCount);
                int[] points = new int[pointCount];
                Marshal.Copy(sysPts, points, 0, pointCount);
                mesh.SetIndices(points, MeshTopology.Points, 0);
            }
            else if (lineCount > 0)
            {
                System.IntPtr sysLines = GlobalOsgInterface.readLines(subID, ref lineCount);
                int[] lines = new int[lineCount];
                Marshal.Copy(sysLines, lines, 0, lineCount);
                mesh.SetIndices(lines, MeshTopology.Lines, 0);
            }

            // Compute bounds
            mesh.RecalculateBounds();
            if (triangleCount > 0 && !hasNormals)
                mesh.RecalculateNormals();
            return mesh;
        }

        private Texture2D CreateTexture(int subID, string texName)
        {
            int texW = 0, texH = 0;
            GlobalOsgInterface.TextureWrap wrap = GlobalOsgInterface.TextureWrap.Clamp;
            GlobalOsgInterface.TextureFormat pf = GlobalOsgInterface.readTextureFormat(subID, ref wrap);
            if (!GlobalOsgInterface.readTextureSize(subID, ref texW, ref texH)) return null;
            
            TextureFormat format = TextureFormat.RGBA32;
            switch (pf)
            {
                case GlobalOsgInterface.TextureFormat.Luminance: format = TextureFormat.Alpha8; break;
                case GlobalOsgInterface.TextureFormat.RGB: format = TextureFormat.RGB24; break;
                case GlobalOsgInterface.TextureFormat.RGBA: format = TextureFormat.RGBA32; break;
                case GlobalOsgInterface.TextureFormat.Dxt1:
                case GlobalOsgInterface.TextureFormat.Dxt1Alpha:
                    format = TextureFormat.DXT1; break;
                case GlobalOsgInterface.TextureFormat.Dxt5: format = TextureFormat.DXT5; break;
                default: break;
            }

            int dataSize = 0;
            System.IntPtr dataPtr = GlobalOsgInterface.readTexture(subID, ref dataSize);
            if (dataSize == 0) return null;

            Texture2D tex2D = new Texture2D(texW, texH, format, false);
            tex2D.LoadRawTextureData(dataPtr, dataSize);
            tex2D.Apply();
            return tex2D;
        }
        
        private Matrix4x4 CreateMatrixFromPtr(System.IntPtr sys, int count)
        {
            float[] ptr = new float[count];
            Marshal.Copy(sys, ptr, 0, count);

            Matrix4x4 matrix = new Matrix4x4();
            matrix.m00 = ptr[0]; matrix.m01 = ptr[1]; matrix.m02 = ptr[2]; matrix.m03 = ptr[3];
            matrix.m10 = ptr[4]; matrix.m11 = ptr[5]; matrix.m12 = ptr[6]; matrix.m13 = ptr[7];
            matrix.m20 = ptr[8]; matrix.m21 = ptr[9]; matrix.m22 = ptr[10]; matrix.m23 = ptr[11];
            matrix.m30 = ptr[12]; matrix.m31 = ptr[13]; matrix.m32 = ptr[14]; matrix.m33 = ptr[15];
            return matrix;
        }

        private void AttachMeshRenderer(GameObject obj, Mesh mesh, ref List<Material> materials)
        {
            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            BoxCollider collider = obj.AddComponent<BoxCollider>();
            collider.center = mesh.bounds.center;
            collider.size = mesh.bounds.size;

            MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
            if (materials.Count > 0) renderer.sharedMaterials = materials.ToArray();
            else renderer.sharedMaterial = standardMaterial;
        }
        
        private void DestroyChildPagedNodes(Transform toRemove, ref Dictionary<int, GameObject> pagedMap)
        {
            Transform[] childrenToRemove = toRemove.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in childrenToRemove)
            {
                if (!pagedMap.ContainsValue(child.gameObject)) continue;
                int key = pagedMap.FirstOrDefault(x => x.Value == child.gameObject).Key;
                pagedMap.Remove(key);

                MeshFilter[] filters = child.GetComponentsInChildren<MeshFilter>();
                if (filters.Length > 0) foreach (MeshFilter mf in filters) Destroy(mf.sharedMesh);
            }
        }
    }
}
