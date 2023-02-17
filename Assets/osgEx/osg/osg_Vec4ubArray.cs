using System.IO;
using UnityEngine;

namespace osgEx 
{
    public class osg_Vec4ubArray : osg_Array
    {
        public Color[] vArray; 
        protected override void read(  BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            int numData = reader.ReadInt32();

             vArray = new Color[numData];   
            for (int i = 0; i < numData; ++i)
            {
                Color v = new Color(
                    (float)reader.ReadByte() / 255.0f, (float)reader.ReadByte() / 255.0f,
                    (float)reader.ReadByte() / 255.0f, (float)reader.ReadByte() / 255.0f);
                vArray[i] = v;
            } 
        }
    }
}
