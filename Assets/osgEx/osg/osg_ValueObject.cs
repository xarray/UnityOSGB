using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace osgEx
{
    public class osg_ValueObject<T> : osg_Object
    {
        public T value;
    }
    public class osg_StringValueObject : osg_ValueObject<string>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            value = ReadString(reader);
        }
    }
    public class osg_BoolValueObject : osg_ValueObject<bool>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            value = reader.ReadBoolean();
        }
    }
    public class osg_CharValueObject : osg_ValueObject<sbyte>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            value = reader.ReadSByte();
        }
    }
    public class osg_UCharValueObject : osg_ValueObject<byte>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            value = reader.ReadByte();
        }
    }
    public class osg_ShortValueObject : osg_ValueObject<short>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            value = reader.ReadInt16();
        }
    }
    public class osg_UShortValueObject : osg_ValueObject<ushort>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            value = reader.ReadUInt16();
        }
    }
    public class osg_IntValueObject : osg_ValueObject<int>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            value = reader.ReadInt32();
        }
    }
    public class osg_UIntValueObject : osg_ValueObject<uint>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            value = reader.ReadUInt32();
        }
    }
    public class osg_FloatValueObject : osg_ValueObject<float>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            value = reader.ReadSingle();
        }
    }
    public class osg_DoubleValueObject : osg_ValueObject<double>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            value = reader.ReadDouble();
        }
    }
    public class osg_Vec2fValueObject : osg_ValueObject<Vector2>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            value = new Vector2(x, y);
        }
    }
    public class osg_Vec3fValueObject : osg_ValueObject<Vector3>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            value = new Vector3(x, y, z);
        }
    }
    public class osg_Vec4fValueObject : osg_ValueObject<Vector4>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            var w = reader.ReadSingle();
            value = new Vector4(x, y, z, w);
        }
    }
    public class osg_Vec2dValueObject : osg_ValueObject<Vector2>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            var x = (float)reader.ReadDouble();
            var y = (float)reader.ReadDouble();
            value = new Vector2(x, y);
        }
    }
    public class osg_Vec3dValueObject : osg_ValueObject<Vector3>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            var x = (float)reader.ReadDouble();
            var y = (float)reader.ReadDouble();
            var z = (float)reader.ReadDouble();
            value = new Vector3(x, y, z);
        }
    }
    public class osg_Vec4dValueObject : osg_ValueObject<Vector4>
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            var x = (float)reader.ReadDouble();
            var y = (float)reader.ReadDouble();
            var z = (float)reader.ReadDouble();
            var w = (float)reader.ReadDouble();
            value = new Vector4(x, y, z, w);
        }
    }


}
