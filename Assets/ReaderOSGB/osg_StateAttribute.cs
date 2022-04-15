using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_StateAttribute : osg_Object
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            bool hasUpdateCB = reader.ReadBoolean();  // _updateCallback
            if (hasUpdateCB) LoadObject(gameObj, reader, owner);

            bool hasEventCB = reader.ReadBoolean();  // _eventCallback
            if (hasEventCB) LoadObject(gameObj, reader, owner);

            return true;
        }
    }
}
