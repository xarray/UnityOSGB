using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Material : osg_StateAttribute
    {
        void readMaterialProperty(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            bool frontAndBack = reader.ReadBoolean();
            Vector4 frontProp = new Vector4(
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector4 backProp = new Vector4(
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            int colorMode = reader.ReadInt32();  // _colorMode

            bool hasMtlProp = reader.ReadBoolean();  // _ambient
            if (hasMtlProp) readMaterialProperty(gameObj, reader, owner);

            hasMtlProp = reader.ReadBoolean();  // _diffuse
            if (hasMtlProp) readMaterialProperty(gameObj, reader, owner);

            hasMtlProp = reader.ReadBoolean();  // _specular
            if (hasMtlProp) readMaterialProperty(gameObj, reader, owner);

            hasMtlProp = reader.ReadBoolean();  // _emission
            if (hasMtlProp) readMaterialProperty(gameObj, reader, owner);

            hasMtlProp = reader.ReadBoolean();  // _shininess
            if (hasMtlProp)
            {
                bool frontAndBack = reader.ReadBoolean();
                float frontValue = reader.ReadSingle();
                float backValue = reader.ReadSingle();
            }

            return true;
        }
    }
}
