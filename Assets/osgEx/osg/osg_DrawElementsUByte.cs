using System.IO;

namespace osgEx
{
    public class osg_DrawElementsUByte : osg_PrimitiveSet
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            numElements = reader.ReadInt32();
            indices = new int[numElements];

            for (uint n = 0; n < numElements; ++n)
            {
                uint value = reader.ReadByte();
                indices[n] = ((int)value);
                if (maxIndex < (int)value) maxIndex = (int)value;
            } 
        }
    }
}
