using System.IO; 

namespace osgEx
{
    public class osg_NodeCallback : osg_Callback
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner); 
        }
    }
}
