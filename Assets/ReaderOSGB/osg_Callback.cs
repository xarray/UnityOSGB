using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Callback : osg_Object
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            bool hasNested = reader.ReadBoolean();  // _nestedCallback
            if (hasNested) LoadObject(gameObj, reader, owner);
            return true;
        }
    }
}
