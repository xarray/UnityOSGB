using System.IO; 

namespace osgEx
{
    public class osg_DrawArrays : osg_PrimitiveSet
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            int first = reader.ReadInt32();  // First
                                             // Count 
            numElements = reader.ReadInt32();
            indices = new int[numElements];

            for (int i = 0; i < numElements; ++i)
            {
                int id = first + i;
                indices[i] = id;
                if (maxIndex < id) maxIndex = id;
            } 
        }
    }
}
