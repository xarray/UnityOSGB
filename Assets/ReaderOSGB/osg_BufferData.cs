using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_BufferData : osg_Object
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            if (owner._version >= 147)
            {
                bool hasBufferObject = reader.ReadBoolean();  // BufferObject
                if (hasBufferObject) LoadObject(gameObj, reader, owner);
            }
            return true;
        }
    }
}
