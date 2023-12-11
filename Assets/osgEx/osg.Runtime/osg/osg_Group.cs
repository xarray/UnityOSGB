using System.IO;

namespace osgEx
{
    public class osg_Group : osg_Node
    {
        public osg_Node[] children;
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            // _children
            bool hasChildren = reader.ReadBoolean();
            if (hasChildren)
            {
                uint numChildren = reader.ReadUInt32();
                long blockSize = ReadBracket(reader, owner);
                children = new osg_Node[numChildren];
                for (uint i = 0; i < numChildren; ++i)
                {
                    children[i] = LoadObject(reader, owner) as osg_Node;
                }
            }
        }
    }
}
