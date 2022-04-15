using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_ClusterCullingCallback : osg_NodeCallback
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            Vector3 cp = new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                                     reader.ReadSingle());  // _controlPoint
            Vector3 normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                                         reader.ReadSingle());  // _normal
            float radius = reader.ReadSingle();  // _radius
            float deviation = reader.ReadSingle();  // _deviation
            return true;
        }
    }
}
