using System.IO;

namespace osgEx
{
    public class osg_Geode : osg_Node
    {
        public osg_Geometry[] drawables;
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            bool hasChildren = reader.ReadBoolean();  // _drawables
            if (hasChildren)
            {
                uint numChildren = reader.ReadUInt32();
                long blockSize = ReadBracket(reader, owner);
                drawables = new osg_Geometry[numChildren];
                for (uint i = 0; i < numChildren; ++i)
                {
                    drawables[i] = LoadObject(reader, owner) as osg_Geometry; 
                }
            } 
        }
    }
}
