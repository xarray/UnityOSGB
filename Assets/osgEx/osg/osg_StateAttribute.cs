using System.IO;

namespace osgEx
{
    public class osg_StateAttribute : osg_Object
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            bool hasUpdateCB = reader.ReadBoolean();  // _updateCallback
            if (hasUpdateCB) LoadObject(reader, owner);

            bool hasEventCB = reader.ReadBoolean();  // _eventCallback
            if (hasEventCB) LoadObject(reader, owner);

        }
    }
}
