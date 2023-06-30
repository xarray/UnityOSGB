using System.IO;

namespace osgEx
{
    public class osg_Geode : osg_Node
    {
        public osg_Geometry[] geometrys;
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            bool hasChildren = reader.ReadBoolean();  // _drawables
            if (hasChildren)
            {
                uint numChildren = reader.ReadUInt32();
                long blockSize = ReadBracket(reader, owner);
                geometrys = new osg_Geometry[numChildren];
                for (uint i = 0; i < numChildren; ++i)
                {
                    geometrys[i] = LoadObject(reader, owner) as osg_Geometry; 
                }
            } 
        }
    }
}
