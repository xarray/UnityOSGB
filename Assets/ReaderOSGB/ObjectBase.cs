using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class ObjectBase
    {
#if UNITY_WEBGL || UNITY_EDITOR
        public static Dictionary<string, System.Func<ObjectBase>> ctorFunc =
            new Dictionary<string, System.Func<ObjectBase>>() {
            { "osg_Object", () => { return new osg_Object(); } },
            { "osg_LOD", () => { return new osg_LOD(); } },
            { "osg_Node", () => { return new osg_Node(); } },
            { "osg_PagedLOD", () => { return new osg_PagedLOD(); } },
            { "osg_Group", () => { return new osg_Group(); } },
            { "osg_Transform", () => { return new osg_Transform(); } },
            { "osg_MatrixTransform", () => { return new osg_MatrixTransform(); } },
            { "osg_Geode", () => { return new osg_Geode(); } },
            { "osg_Drawable", () => { return new osg_Drawable(); } },
            { "osg_Drawable_2", () => { return new osg_Drawable_2(); } },
            { "osg_Geometry", () => { return new osg_Geometry(); } },
            { "osg_Geometry_2", () => { return new osg_Geometry_2(); } },
            { "osg_BufferData", () => { return new osg_BufferData(); } },
            { "osg_DrawArrays", () => { return new osg_DrawArrays(); } },
            { "osg_DrawElementsUByte", () => { return new osg_DrawElementsUByte(); } },
            { "osg_DrawElementsUShort", () => { return new osg_DrawElementsUShort(); } },
            { "osg_DrawElementsUInt", () => { return new osg_DrawElementsUInt(); } },
            { "osg_Vec2Array", () => { return new osg_Vec2Array(); } },
            { "osg_Vec3Array", () => { return new osg_Vec3Array(); } },
            { "osg_Vec4Array", () => { return new osg_Vec4Array(); } },
            { "osg_Vec4ubArray", () => { return new osg_Vec4ubArray(); } },
            { "osg_StateSet", () => { return new osg_StateSet(); } },
            { "osg_StateAttribute", () => { return new osg_StateAttribute(); } },
            { "osg_Material", () => { return new osg_Material(); } },
            { "osg_Texture", () => { return new osg_Texture(); } },
            { "osg_Texture2D", () => { return new osg_Texture2D(); } },
        };
#endif

        public static string ReadString(BinaryReader reader)
        {
            int strLength = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(strLength);
            return System.Text.Encoding.UTF8.GetString(bytes);
            //char[] compressor = reader.ReadChars(strLength);
            //return new string(compressor);
        }

        public static long ReadBracket(BinaryReader reader, ReaderOSGB owner)
        {
            if (owner._useBrackets)
            {
                if (owner._version > 148) return reader.ReadInt64();
                else return reader.ReadInt32();
            }
            return -1;
        }

        public static Texture2D LoadImage(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            Texture2D tex2D = null;
            if (owner._version > 94) { string className = ReadString(reader); }

            uint id = reader.ReadUInt32();
            if (owner._sharedTextures.ContainsKey(id))
                return owner._sharedTextures[id];

            string fileName = ReadString(reader);
            int writeHint = reader.ReadInt32();
            int decision = reader.ReadInt32();
            switch (decision)
            {
                case 0:  // IMAGE_INLINE_DATA
                    {
                        int origin = reader.ReadInt32();
                        int s = reader.ReadInt32();
                        int t = reader.ReadInt32();
                        int r = reader.ReadInt32();
                        int internalFormat = reader.ReadInt32();
                        int pixelFormat = reader.ReadInt32();
                        int dataType = reader.ReadInt32();
                        int packing = reader.ReadInt32();
                        int mode = reader.ReadInt32();

                        uint size = reader.ReadUInt32();
                        byte[] imageData = reader.ReadBytes((int)size);
                        if (size > 0)
                        {
                            TextureFormat format = TextureFormat.RGB24;  // TODO: other formats/data size
                            if (dataType == 0x1401)
                            {
                                if (pixelFormat == 0x1907) format = TextureFormat.RGB24;  // GL_RGB
                                else if (pixelFormat == 0x1908) format = TextureFormat.RGBA32;  // GL_RGBA
                                else if (pixelFormat == 0x80E1) format = TextureFormat.BGRA32;  // GL_BGRA
                                else if (pixelFormat == 0x83F0)
                                    format = TextureFormat.DXT1;  // GL_COMPRESSED_RGB_S3TC_DXT1_EXT
                                else if (pixelFormat == 0x83F3)
                                    format = TextureFormat.DXT5;  // GL_COMPRESSED_RGB_S3TC_DXT5_EXT
                                else Debug.LogWarning("Unsupported texture pixel format " + pixelFormat);
                            }
                            else
                                Debug.LogWarning("Unsupported texture data type " + dataType);

                            tex2D = new Texture2D(s, t, format, false);
                            tex2D.LoadRawTextureData(imageData);
                            tex2D.Apply();
                        }

                        uint numLevels = reader.ReadUInt32();
                        for (uint i = 0; i < numLevels; ++i)
                        {
                            uint levelDataSize = reader.ReadUInt32();
                            // TODO
                        }
                    }
                    break;
                case 1:  // IMAGE_INLINE_FILE
                    {
                        uint size = reader.ReadUInt32();
                        if (size > 0)
                        {
                            byte[] fileData = reader.ReadBytes((int)size);

                            tex2D = new Texture2D(2, 2);
                            tex2D.LoadImage(fileData);
                            //File.WriteAllBytes("test.jpg", fileData);
                        }
                    }
                    break;
                case 2:  // IMAGE_EXTERNAL
                    if (File.Exists(fileName))
                    {
                        byte[] fileData = File.ReadAllBytes(fileName);
                        tex2D = new Texture2D(2, 2);
                        tex2D.LoadImage(fileData);
                    }
                    else
                        Debug.LogWarning("Image file '" + fileName + "' not found");
                    break;
                default: break;
            }

            osg_Object classObj = new osg_Object();
            classObj.read(gameObj, reader, owner);
            owner._sharedTextures[id] = tex2D;
            return tex2D;
        }

        public static bool LoadObject(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            string className = ReadString(reader);
            long blockSize = ReadBracket(reader, owner);
            uint id = reader.ReadUInt32();
            className = className.Replace("::", "_");

            if (owner._sharedObjects.ContainsKey(id))
            {
                //Debug.Log("Shared object " + className + "-" + id);
                return true;  // TODO: how to share nodes?
            }
            else
                owner._sharedObjects[id] = gameObj;

            if (owner._version < 154)
            {
                if (className == "osg_Geometry") className = "osg_Geometry_2";
                else if (className == "osg_Drawable") className = "osg_Drawable_2";
            }
            //\Debug.Log(className + " - " + id);

#if UNITY_WEBGL || UNITY_EDITOR
            System.Func<ObjectBase> ctor = null;
            if (!ctorFunc.TryGetValue(className, out ctor))
            {
                Debug.LogWarning("Object type " + className + " not implemented");
                if (blockSize != -1) reader.BaseStream.Position += (blockSize - 4);
                return false;
            }

            ObjectBase classObj = ctor.Invoke();
#else
            System.Type classType = System.Type.GetType("osgEx." + className);
            if (classType == null)
            {
                Debug.LogWarning("Object type " + className + " not implemented");
                if (blockSize != -1) reader.BaseStream.Position += (blockSize - 4);
                return false;
            }

            ObjectBase classObj = System.Activator.CreateInstance(classType) as ObjectBase;
#endif
            if (classObj == null)
            {
                Debug.LogWarning("Object instance " + className + " failed to create");
                if (blockSize != -1) reader.BaseStream.Position += (blockSize - 4);
                return false;
            }
            else
                return classObj.read(gameObj, reader, owner);
        }

        public virtual bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        { Debug.LogWarning("Method read() not implemented"); return false; }
    }
}
