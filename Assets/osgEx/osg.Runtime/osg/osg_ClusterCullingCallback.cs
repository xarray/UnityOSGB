using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_ClusterCullingCallback : osg_NodeCallback
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            Vector3 cp = new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                                     reader.ReadSingle());  // _controlPoint
            Vector3 normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                                         reader.ReadSingle());  // _normal
            float radius = reader.ReadSingle();  // _radius
            float deviation = reader.ReadSingle();  // _deviation 
        }
    }
}
