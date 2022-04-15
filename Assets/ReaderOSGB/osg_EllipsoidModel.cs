using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_EllipsoidModel : osg_Object
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            double radiusE = reader.ReadDouble();  // _radiusEquator
            double radiusP = reader.ReadDouble();  // _radiusPolar
            return true;
        }
    }
}
