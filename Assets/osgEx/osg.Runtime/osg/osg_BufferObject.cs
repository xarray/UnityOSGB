using System.IO;

namespace osgEx
{
    public class osg_BufferObject : osg_Object
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            int type = reader.ReadInt32();  // _type
            int usage = reader.ReadInt32();  // _usage
            bool cdr = reader.ReadBoolean();  // CopyDataAndReleaseGLBufferObject
            if (owner._version >= 201)
            {
                int mappingBitField = reader.ReadInt32();  // _mappingBitField
            } 
        }
    }
}
