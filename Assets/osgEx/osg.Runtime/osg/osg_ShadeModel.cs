using System.IO; 

namespace osgEx
{
    public class osg_ShadeModel : osg_StateAttribute
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            int shadeMode = reader.ReadInt32();  // _mode 
        }
    }
}
