using System.IO;

namespace osgEx
{
    public class osg_BufferData : osg_Object
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            if (owner._version >= 147)
            {
                bool hasBufferObject = reader.ReadBoolean();  // BufferObject
                if (hasBufferObject) LoadObject(reader, owner);
            }
        }
    }
}
