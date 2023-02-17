using System.IO;

namespace osgEx
{

    public class osg_Array : osg_BufferData  // FIXME: version >= 147
    {
        int binding;  // Binding
        bool normalize;  // Normalize
        bool preserveDataType;  // PreserveDataType
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            binding = reader.ReadInt32();  // Binding
            normalize = reader.ReadBoolean();  // Normalize
            preserveDataType = reader.ReadBoolean();  // PreserveDataType  
        }
    }
}
