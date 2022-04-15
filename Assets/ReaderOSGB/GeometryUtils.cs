using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace osgEx
{
    public class GeometryUtils
    {
        public static uint readPrimitive(ref List<int> indices, BinaryReader reader, ReaderOSGB owner)
        {
            int type = reader.ReadInt32();
            int mode = reader.ReadInt32(), maxIndex = 0;
            if (owner._version > 96) { uint numInstances = reader.ReadUInt32(); }

            List<int> localIndices = new List<int>();
            switch (type)
            {
                case 50:  // ID_DRAWARRAYS
                    {
                        int first = reader.ReadInt32();
                        int count = reader.ReadInt32();
                        for (int i = 0; i < count; ++i)
                        {
                            int id = first + i; localIndices.Add(id);
                            if (maxIndex < id) maxIndex = id;
                        }
                    }
                    break;
                case 51:  // ID_DRAWARRAY_LENGTH
                    {
                        int first = reader.ReadInt32();
                        uint numArrays = reader.ReadUInt32();
                        long blockSize = ObjectBase.ReadBracket(reader, owner);
                        for (uint n = 0; n < numArrays; ++n)
                        {
                            int value = reader.ReadInt32();
                            // TODO
                        }
                    }
                    break;
                case 52:  // ID_DRAWELEMENTS_UBYTE
                    {
                        uint numElements = reader.ReadUInt32();
                        long blockSize = ObjectBase.ReadBracket(reader, owner);
                        for (uint n = 0; n < numElements; ++n)
                        {
                            uint value = reader.ReadByte(); localIndices.Add((int)value);
                            if (maxIndex < (int)value) maxIndex = (int)value;
                        }
                    }
                    break;
                case 53:  // ID_DRAWELEMENTS_USHORT
                    {
                        uint numElements = reader.ReadUInt32();
                        long blockSize = ObjectBase.ReadBracket(reader, owner);
                        for (uint n = 0; n < numElements; ++n)
                        {
                            uint value = reader.ReadUInt16(); localIndices.Add((int)value);
                            if (maxIndex < (int)value) maxIndex = (int)value;
                        }
                    }
                    break;
                case 54:  // ID_DRAWELEMENTS_UINT
                    {
                        uint numElements = reader.ReadUInt32();
                        long blockSize = ObjectBase.ReadBracket(reader, owner);
                        for (uint n = 0; n < numElements; ++n)
                        {
                            uint value = reader.ReadUInt32(); localIndices.Add((int)value);
                            if (maxIndex < (int)value) maxIndex = (int)value;
                        }
                    }
                    break;
                default: break;
            }

            switch (mode)
            {
                case 4:  // TRIANGLES
                    indices.AddRange(localIndices);
                    break;
                case 5:  // TRIANGLE_STRIP
                    for (int i = 2; i < localIndices.Count; ++i)
                    {
                        if ((i % 2) == 0)
                        {
                            indices.Add(localIndices[i - 2]);
                            indices.Add(localIndices[i - 1]);
                        }
                        else
                        {
                            indices.Add(localIndices[i - 1]);
                            indices.Add(localIndices[i - 2]);
                        }
                        indices.Add(localIndices[i]);
                    }
                    break;
                case 6:  // TRIANGLE_FAN
                    for (int i = 2; i < localIndices.Count; ++i)
                    {
                        indices.Add(localIndices[0]);
                        indices.Add(localIndices[i - 1]);
                        indices.Add(localIndices[i]);
                    }
                    break;
                default:
                    Debug.LogWarning("Unsupported primitive mode " + mode);
                    break;
            }
            return (uint)maxIndex;
        }

        public static byte[] readArray(BinaryReader reader, ReaderOSGB owner,
                                       ref int numComponents, ref int dataSize, ref int arraySize)
        {
            if (owner._version < 112)
            {
                uint id = reader.ReadUInt32();
                int type = reader.ReadInt32();
                switch (type)
                {
                    case 0: numComponents = 1; dataSize = 1; break;  // ID_BYTE_ARRAY
                    case 1: numComponents = 1; dataSize = 1; break;  // ID_UBYTE_ARRAY
                    case 2: numComponents = 1; dataSize = 2; break;  // ID_SHORT_ARRAY
                    case 3: numComponents = 1; dataSize = 2; break;  // ID_USHORT_ARRAY
                    case 4: numComponents = 1; dataSize = 4; break;  // ID_INT_ARRAY
                    case 5: numComponents = 1; dataSize = 4; break;  // ID_UINT_ARRAY
                    case 6: numComponents = 1; dataSize = 4; break;  // ID_FLOAT_ARRAY
                    case 7: numComponents = 1; dataSize = 8; break;  // ID_DOUBLE_ARRAY
                    case 8: numComponents = 2; dataSize = 1; break;  // ID_VEC2B_ARRAY
                    case 9: numComponents = 3; dataSize = 1; break;  // ID_VEC3B_ARRAY
                    case 10: numComponents = 4; dataSize = 1; break;  // ID_VEC4B_ARRAY
                    case 11: numComponents = 4; dataSize = 1; break;  // ID_VEC4UB_ARRAY
                    case 12: numComponents = 2; dataSize = 2; break;  // ID_VEC2S_ARRAY
                    case 13: numComponents = 3; dataSize = 2; break;  // ID_VEC3S_ARRAY
                    case 14: numComponents = 4; dataSize = 2; break;  // ID_VEC4S_ARRAY
                    case 15: numComponents = 2; dataSize = 4; break;  // ID_VEC2_ARRAY
                    case 16: numComponents = 3; dataSize = 4; break;  // ID_VEC3_ARRAY
                    case 17: numComponents = 4; dataSize = 4; break;  // ID_VEC4_ARRAY
                    case 18: numComponents = 2; dataSize = 8; break;  // ID_VEC2D_ARRAY
                    case 19: numComponents = 3; dataSize = 8; break;  // ID_VEC3D_ARRAY
                    case 20: numComponents = 4; dataSize = 8; break;  // ID_VEC4D_ARRAY
                    case 21: numComponents = 2; dataSize = 1; break;  // ID_VEC2UB_ARRAY
                    case 22: numComponents = 3; dataSize = 1; break;  // ID_VEC3UB_ARRAY
                    case 23: numComponents = 2; dataSize = 2; break;  // ID_VEC2US_ARRAY
                    case 24: numComponents = 3; dataSize = 2; break;  // ID_VEC3US_ARRAY
                    case 25: numComponents = 4; dataSize = 2; break;  // ID_VEC4US_ARRAY
                    case 26: numComponents = 2; dataSize = 4; break;  // ID_VEC2I_ARRAY
                    case 27: numComponents = 3; dataSize = 4; break;  // ID_VEC3I_ARRAY
                    case 28: numComponents = 4; dataSize = 4; break;  // ID_VEC4I_ARRAY
                    case 29: numComponents = 2; dataSize = 4; break;  // ID_VEC2UI_ARRAY
                    case 30: numComponents = 3; dataSize = 4; break;  // ID_VEC3UI_ARRAY
                    case 31: numComponents = 4; dataSize = 4; break;  // ID_VEC4UI_ARRAY
                    default: break;
                }

                arraySize = reader.ReadInt32();
                long blockSize = ObjectBase.ReadBracket(reader, owner);
                return reader.ReadBytes(arraySize * numComponents * dataSize);
            }
            else
            { }  // TODO
            return null;
        }

        public static void readArrayData(string name, Mesh mesh, BinaryReader reader, ReaderOSGB owner)
        {
            long blockSize = ObjectBase.ReadBracket(reader, owner);
            bool hasArray = reader.ReadBoolean();
            if (hasArray)
            {
                int numComponents = 0, dataSize = 0, arraySize = 0;
                byte[] data = readArray(reader, owner, ref numComponents, ref dataSize, ref arraySize);

                if (name == "vertex")
                {
                    Vector3[] vertices = new Vector3[arraySize];
                    GCHandle handle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
                    Marshal.Copy(data, 0, handle.AddrOfPinnedObject(), data.Length);
                    mesh.vertices = vertices;
                }
                else if (name == "normal")
                {
                    Vector3[] normals = new Vector3[arraySize];
                    GCHandle handle = GCHandle.Alloc(normals, GCHandleType.Pinned);
                    Marshal.Copy(data, 0, handle.AddrOfPinnedObject(), data.Length);
                    mesh.normals = normals;
                }
                else if (name == "texCoord0")
                {
                    Vector2[] texcoords = new Vector2[arraySize];
                    GCHandle handle = GCHandle.Alloc(texcoords, GCHandleType.Pinned);
                    Marshal.Copy(data, 0, handle.AddrOfPinnedObject(), data.Length);
                    mesh.uv = texcoords;
                }
                else  // TODO: other types
                    Debug.LogWarning("Not implemented: " + name + ", " + arraySize +
                                     "; components = " + numComponents + "; dataSize = " + dataSize);
            }

            bool hasIndices = reader.ReadBoolean();
            if (hasIndices)
            {
                int numComponents = 0, dataSize = 0, arraySize = 0;
                byte[] data = readArray(reader, owner, ref numComponents, ref dataSize, ref arraySize);
            }

            int binding = reader.ReadInt32();
            int normalizeValue = reader.ReadInt32();
        }

        public static void readArrayListData(string name, Mesh mesh, BinaryReader reader, ReaderOSGB owner)
        {
            uint numArrayData = reader.ReadUInt32();
            long blockSize = ObjectBase.ReadBracket(reader, owner);
            for (uint i = 0; i < numArrayData; ++i)
            {
                blockSize = ObjectBase.ReadBracket(reader, owner);
                readArrayData(name + i.ToString(), mesh, reader, owner);
            }
        }
    }
}
