using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_ShadeModel : osg_StateAttribute
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            int shadeMode = reader.ReadInt32();  // _mode
            return true;
        }
    }
}
