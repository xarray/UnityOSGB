using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_CoordinateSystemNode : osg_Group
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            string format = ReadString(reader);  // _format
            string cs = ReadString(reader);  // _cs

            bool hasEllipsoid = reader.ReadBoolean();  // _ellipsoidModel
            if (hasEllipsoid) LoadObject(gameObj, reader, owner);
            return true;
        }
    }
}
