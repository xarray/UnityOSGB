using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_BufferObject : osg_Object
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            int type = reader.ReadInt32();  // _type
            int usage = reader.ReadInt32();  // _usage
            bool cdr = reader.ReadBoolean();  // CopyDataAndReleaseGLBufferObject
            if (owner._version >= 201)
            {
                int mappingBitField = reader.ReadInt32();  // _mappingBitField
            }

            return true;
        }
    }
}
