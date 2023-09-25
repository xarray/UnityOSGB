using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Vec4Array : osg_Array
    {
        public Vector4[] vArray;
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            int numData = reader.ReadInt32();
            vArray = new Vector4[numData];
            for (int i = 0; i < numData; ++i)
            {
                Vector4 v = new Vector4(reader.ReadSingle(), reader.ReadSingle(),
                                        reader.ReadSingle(), reader.ReadSingle());
                vArray[i] = v;
            } 
        }
    }
}
