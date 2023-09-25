using System.IO;

namespace osgEx
{
    public class osg_Callback : osg_Object
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            bool hasNested = reader.ReadBoolean();  // _nestedCallback
            if (hasNested) LoadObject(reader, owner);
        }
    }
}
