using System.IO; 

namespace osgEx
{
    public class osg_PrimitiveSet : osg_BufferData  // FIXME: version >= 147
    {
        public int numInstances;
        public int mode;
        public int numElements;
        public int maxIndex;
        public int[] indices;
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            numInstances = reader.ReadInt32();  // NumInstances
            mode = reader.ReadInt32();  // Mode  
        }
    }
}
