using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Vec2Array : osg_Array
    {
        public Vector2[] vArray;
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            int numData = reader.ReadInt32();
            vArray = new Vector2[numData];
            for (int i = 0; i < numData; ++i)
            {
                vArray[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            }
        }
    }
}
