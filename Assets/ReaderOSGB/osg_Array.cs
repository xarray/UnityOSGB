using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Array : osg_BufferData  // FIXME: version >= 147
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            int binding = reader.ReadInt32();  // Binding
            bool normalize = reader.ReadBoolean();  // Normalize
            bool preserveDataType = reader.ReadBoolean();  // PreserveDataType

            return true;
        }
    }
}
