using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Object
    {
        public static Dictionary<string, Func<osg_Object>> ctorFunc = new Dictionary<string, Func<osg_Object>>() {
            { "osg::Object",()=>{return new osg_Object(); } },
            { "osg::LOD",()=>{return new osg_LOD(); } },
            { "osg::Node",()=>{return new osg_Node(); } },
            { "osg::PagedLOD",()=>{return new osg_PagedLOD(); } },
            { "osg::Group",()=>{return new osg_Group(); } },
            { "osg::Transform",()=>{return new osg_Transform(); } },
            { "osg::MatrixTransform",()=>{return new osg_MatrixTransform(); } },
            { "osg::Geode",()=>{return new osg_Geode(); } },
            { "osg::Drawable",()=>{return new osg_Drawable(); } },
            { "osg::Geometry",()=>{return new osg_Geometry(); } },
            { "osg::BufferData",()=>{return new osg_BufferData(); } },

            { "osg::DrawArrays",()=>{return new osg_DrawArrays(); } },
            { "osg::DrawElementsUByte",()=>{return new osg_DrawElementsUByte(); } },
            { "osg::DrawElementsUShort",()=>{return new osg_DrawElementsUShort(); } },
            { "osg::DrawElementsUInt",()=>{return new osg_DrawElementsUInt(); } },

            { "osg::Vec2Array",()=>{return new osg_Vec2Array(); } },
            { "osg::Vec3Array",()=>{return new osg_Vec3Array(); } },
            { "osg::Vec4Array",()=>{return new osg_Vec4Array(); } },
            { "osg::Vec4ubArray",()=>{return new osg_Vec4ubArray(); } },

            { "osg::StateSet",()=>{return new osg_StateSet(); } },
            { "osg::StateAttribute",()=>{return new osg_StateAttribute(); } },
            { "osg::Material",()=>{return new osg_Material(); } },
            { "osg::Texture",()=>{return new osg_Texture(); } },
            { "osg::Texture2D",()=>{return new osg_Texture2D(); } },

            { "osg::DefaultUserDataContainer",()=>{return new osg_DefaultUserDataContainer(); } },
            { "osg::UserDataContainer",()=>{return new osg_UserDataContainer(); } },

            { "osg::BoolValueObject",()=>{return new osg_BoolValueObject(); } },
            { "osg::CharValueObject",()=>{return new osg_CharValueObject(); } },
            { "osg::UCharValueObject",()=>{return new osg_UCharValueObject(); } },
            { "osg::ShortValueObject",()=>{return new osg_ShortValueObject(); } },
            { "osg::UShortValueObject",()=>{return new osg_UShortValueObject(); } },
            { "osg::IntValueObject",()=>{return new osg_IntValueObject(); } },
            { "osg::UIntValueObject",()=>{return new osg_UIntValueObject(); } },
            { "osg::FloatValueObject",()=>{return new osg_FloatValueObject(); } },
            { "osg::DoubleValueObject",()=>{return new osg_DoubleValueObject(); } },
            { "osg::StringValueObject",()=>{return new osg_StringValueObject(); } },
            { "osg::Vec2fValueObject",()=>{return new osg_Vec2fValueObject(); } },
            { "osg::Vec3fValueObject",()=>{return new osg_Vec3fValueObject(); } },
            { "osg::Vec4fValueObject",()=>{return new osg_Vec4fValueObject(); } },
            { "osg::Vec2dValueObject",()=>{return new osg_Vec2dValueObject(); } },
            { "osg::Vec3dValueObject",()=>{return new osg_Vec3dValueObject(); } },
            { "osg::Vec4dValueObject",()=>{return new osg_Vec4dValueObject(); } },

        };
        public static string ReadString(BinaryReader reader)
        {
            int strLength = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(strLength);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (BinaryReader strReader = new BinaryReader(stream))
                {
                    char[] compressor = strReader.ReadChars(strLength);
                    string str = new string(compressor);
                    return str;
                }
            }
        }
        public static long ReadBracket(BinaryReader reader, osg_Reader owner)
        {
            long value = -1;
            if (owner._useBrackets)
            {
                if (owner._version > 148)
                {
                    value = reader.ReadInt64();
                    value -= 8;
                    return value;
                }
                else
                {
                    value = reader.ReadInt32();
                    value -= 4;
                    return value;
                }
            }
            return -1;
        }
        public static osg_Object LoadObject(BinaryReader reader, osg_Reader owner)
        {
            string className = ReadString(reader);
            long blockSize = ReadBracket(reader, owner);

            uint id = reader.ReadUInt32();
            if (owner._sharedObjects.TryGetValue(id, out osg_Object temp))
            {
                return temp;
            }
            if (ctorFunc.TryGetValue(className, out Func<osg_Object> ctor))
            {
                osg_Object obj = ctor.Invoke();
                obj.read(reader, owner);
                owner._sharedObjects[id] = obj;
                return obj;
            }
            else if (blockSize != -1)
            {
                reader.BaseStream.Position += (blockSize - 4); //块总大小-描述的大小-id的大小
                Debug.Log(string.Format("未实现{0}类,尝试跳过{1}字节长度 ", className, (blockSize - 4)));
                owner._sharedObjects[id] = null;
                return null;
            }
            else
            {
                throw new Exception(string.Format("未实现{0}类 ", className));
            }
        }

        public string name;
        public int dataVariance;
        public osg_Object userData;
        public osg_Reader owner;
        protected virtual void read(BinaryReader reader, osg_Reader owner)
        {
            this.owner = owner;
            name = ReadString(reader);  // _name
            dataVariance = reader.ReadInt32();  // _dataVariance 
            bool hasUserData = reader.ReadBoolean();  // _userData
            if (hasUserData)
            {
                userData = LoadObject(reader, owner);
            }
        }
    }

}

