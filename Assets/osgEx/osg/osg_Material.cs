using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Material : osg_StateAttribute
    {
        Vector4 readMaterialProperty(BinaryReader reader, osg_Reader owner)
        {
            bool frontAndBack = reader.ReadBoolean();
            Vector4 frontProp = new Vector4(
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector4 backProp = new Vector4(
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            if (frontAndBack)
            {
                return frontProp;
            }
            else
            {
                return backProp;
            }
        }
        public int colorMode;
        public Vector4? ambient;
        public Vector4? diffuse;
        public Vector4? specular;
        public Vector4? emission;

        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            int colorMode = reader.ReadInt32();  // _colorMode

            bool hasMtlProp = reader.ReadBoolean();  // _ambient
            if (hasMtlProp) ambient = readMaterialProperty(reader, owner);

            hasMtlProp = reader.ReadBoolean();  // _diffuse
            if (hasMtlProp) diffuse = readMaterialProperty(reader, owner);

            hasMtlProp = reader.ReadBoolean();  // _specular
            if (hasMtlProp) specular = readMaterialProperty(reader, owner);

            hasMtlProp = reader.ReadBoolean();  // _emission
            if (hasMtlProp) emission = readMaterialProperty(reader, owner);

            hasMtlProp = reader.ReadBoolean();  // _shininess
            if (hasMtlProp)
            {
                bool frontAndBack = reader.ReadBoolean();
                float frontValue = reader.ReadSingle();
                float backValue = reader.ReadSingle();
            }
        }
    }
}
