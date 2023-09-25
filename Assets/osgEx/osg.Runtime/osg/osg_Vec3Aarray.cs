using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Vec3Array : osg_Array
    {
        public Vector3[] vArray;
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner); 
            int numData = reader.ReadInt32();
            vArray=new Vector3[numData];
            for (int i = 0; i < numData; ++i)
            {
                Vector3 v = new Vector3(reader.ReadSingle(),
                                        reader.ReadSingle(), reader.ReadSingle());
                vArray[i] = v;
            } 
        }
    }
}
