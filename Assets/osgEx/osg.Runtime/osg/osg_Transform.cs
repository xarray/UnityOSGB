using System.IO;

namespace osgEx
{
    public class osg_Transform : osg_Group
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            int referenceFrame = reader.ReadInt32();  // _referenceFrame 
        }
    }
}
