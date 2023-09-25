using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;

namespace osgEx
{
    /// <summary>
    /// 网格相关数据
    /// </summary>
    public class osg_Geometry : osg_Drawable
    {
        public int[] indices;
        public Vector3[] vertexs;
        public Vector3[] normals;
        public Vector3[] color;
        public Vector2[][] uv;
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            // _primitives
            uint numPrimitives = reader.ReadUInt32(), maxIndex = 0;
            List<int> _indices = new List<int>();
            for (uint n = 0; n < numPrimitives; ++n)
            {
                if (owner._version < 112)
                {
                    uint id = readPrimitive(ref _indices, reader, owner);
                    if (maxIndex < id) maxIndex = id;
                }
                else
                {
                    osg_PrimitiveSet temp = LoadObject(reader, owner) as osg_PrimitiveSet;

                    switch (temp.mode)
                    {
                        case 4:  // TRIANGLES
                            for (int i = 0; i < temp.indices.Length; i++)
                            {
                                _indices.Add((int)temp.indices[i]);
                            }
                            break;
                        case 5:  // TRIANGLE_STRIP
                            for (int i = 2; i < temp.indices.Length; ++i)
                            {
                                if ((i % 2) == 0)
                                {
                                    _indices.Add((int)temp.indices[i - 2]);
                                    _indices.Add((int)temp.indices[i - 1]);
                                }
                                else
                                {
                                    _indices.Add((int)temp.indices[i - 1]);
                                    _indices.Add((int)temp.indices[i - 2]);
                                }
                                _indices.Add((int)temp.indices[i]);
                            }
                            break;
                        case 6:  // TRIANGLE_FAN
                            for (int i = 2; i < temp.indices.Length; ++i)
                            {
                                _indices.Add((int)temp.indices[0]);
                                _indices.Add((int)temp.indices[i - 1]);
                                _indices.Add((int)temp.indices[i]);
                            }
                            break;
                        default:
                            Debug.LogWarning("Unsupported primitive mode " + temp.mode);
                            break;
                    }
                }

            }
            indices = _indices.ToArray();

            if (owner._version < 112)
            {
                bool hasArrayData = reader.ReadBoolean();  // _vertexData
                if (hasArrayData)
                {
                    vertexs = readArrayData<Vector3>(reader, owner);
                }

                hasArrayData = reader.ReadBoolean();  // _normalData
                if (hasArrayData)
                {
                    normals = readArrayData<Vector3>(reader, owner);
                }
                hasArrayData = reader.ReadBoolean();  // _colorData
                if (hasArrayData)
                {
                    color = readArrayData<Vector3>(reader, owner);
                }
                hasArrayData = reader.ReadBoolean();  // _secondaryColorData
                if (hasArrayData)
                {
                    var secondaryColor = readArrayData<Vector3>(reader, owner);
                }
                hasArrayData = reader.ReadBoolean();  // _fogCoordData
                if (hasArrayData)
                {
                    var fogCoord = readArrayData<Vector2>(reader, owner);
                }
                hasArrayData = reader.ReadBoolean();  // _texCoordList
                if (hasArrayData)
                {
                    uv = readArrayListData<Vector2>(reader, owner);
                }
                hasArrayData = reader.ReadBoolean();  // _vertexAttribList
                if (hasArrayData)
                {
                    var vertexAttrib = readArrayListData<Vector2>(reader, owner);
                }
                bool hasFastPath = reader.ReadBoolean();  // _fastPathHint
                if (hasFastPath) { }  // { bool fastPathHint = reader.ReadBoolean(); }
            }
            else
            {
                bool hasData = reader.ReadBoolean();  // _vertexData
                if (hasData)
                {
                    osg_Vec3Array vec3Array = LoadObject(reader, owner) as osg_Vec3Array;
                    vertexs = vec3Array.vArray;
                }

                hasData = reader.ReadBoolean();  // _normalData
                if (hasData)
                {
                    osg_Vec3Array vec3Array = LoadObject(reader, owner) as osg_Vec3Array;
                    normals = vec3Array.vArray;
                }

                hasData = reader.ReadBoolean();  // _colorData
                if (hasData) LoadObject(reader, owner);
                hasData = reader.ReadBoolean();  // _secondaryColorData
                if (hasData) LoadObject(reader, owner);
                hasData = reader.ReadBoolean();  // _fogCoordData
                if (hasData) LoadObject(reader, owner);

                uint numArrayData = reader.ReadUInt32();  // _texCoordList
                uv = new Vector2[numArrayData][];
                for (uint i = 0; i < numArrayData; ++i)
                {
                    osg_Vec2Array vec2Array = LoadObject(reader, owner) as osg_Vec2Array;
                    uv[i] = vec2Array.vArray;
                }

                numArrayData = reader.ReadUInt32();  // _vertexAttribList
                for (uint i = 0; i < numArrayData; ++i) LoadObject(reader, owner);
            }
        }

        public static uint readPrimitive(ref List<int> indices, BinaryReader reader, osg_Reader owner)
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
                        long blockSize = ReadBracket(reader, owner);
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
                        long blockSize = ReadBracket(reader, owner);
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
                        long blockSize = ReadBracket(reader, owner);
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
                        long blockSize = ReadBracket(reader, owner);
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


        public static T[] readArrayData<T>(BinaryReader reader, osg_Reader owner) where T : struct
        {
            T[] vertices = null;
            long blockSize = ReadBracket(reader, owner);
            bool hasArray = reader.ReadBoolean();
            if (hasArray)
            {
                int numComponents = 0, dataSize = 0, arraySize = 0;
                byte[] data = readArray(reader, owner, ref numComponents, ref dataSize, ref arraySize);
                vertices = new T[arraySize];
                GCHandle handle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
                Marshal.Copy(data, 0, handle.AddrOfPinnedObject(), data.Length);
                handle.Free();
            }
            bool hasIndices = reader.ReadBoolean();
            if (hasIndices)
            {
                int numComponents = 0, dataSize = 0, arraySize = 0;
                byte[] data = readArray(reader, owner, ref numComponents, ref dataSize, ref arraySize);
            }

            int binding = reader.ReadInt32();
            int normalizeValue = reader.ReadInt32();
            return vertices;
        }
        public static byte[] readArray(BinaryReader reader, osg_Reader owner,
                                 ref int numComponents, ref int dataSize, ref int arraySize)
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
            long blockSize = ReadBracket(reader, owner);
            return reader.ReadBytes(arraySize * numComponents * dataSize);
        }

        public static T[][] readArrayListData<T>(BinaryReader reader, osg_Reader owner) where T : struct
        {
            T[][] result = null;
            uint numArrayData = reader.ReadUInt32();
            long blockSize = ReadBracket(reader, owner);
            result = new T[numArrayData][];
            for (uint i = 0; i < numArrayData; ++i)
            {
                //blockSize = ReadBracket(reader, owner);
                result[i] = readArrayData<T>(reader, owner);
            }
            return result;
        }
    }
}
