using System.IO;  

namespace osgEx
{
    public class osg_PagedLOD : osg_LOD
    {
        public string databasePath;
        public int numExpired;
        public bool disableExternalPaging;
        public bool hasRangeData;
        public string[] rangeData;
        public osg_Geode[] children;
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            // _databasePath
            bool hasPath = reader.ReadBoolean();
            if (hasPath)
            {
                bool notEmptyPath = reader.ReadBoolean();
                if (notEmptyPath)
                {
                    databasePath = ReadString(reader);
                }
            }
            // _frameNumberOfLastTraversal
            //int numFrames = reader.ReadInt32();   
            numExpired = reader.ReadInt32();
            disableExternalPaging = reader.ReadBoolean();
            hasRangeData = reader.ReadBoolean();
            if (hasRangeData)
            {
                uint numRanges = reader.ReadUInt32();
                long blockSize = ReadBracket(reader, owner);
                rangeData = new string[numRanges];
                for (uint i = 0; i < numRanges; ++i)
                {
                    string pagedFile = ReadString(reader); 
                    rangeData[i] = pagedFile;
                }
                uint numPriorityOffsets = reader.ReadUInt32();
                blockSize = ReadBracket(reader, owner);
                for (uint i = 0; i < numPriorityOffsets; ++i)
                {
                    float offset = reader.ReadSingle();
                    float scale = reader.ReadSingle();
                }
            }
            bool hasChildren = reader.ReadBoolean();
            if (hasChildren)
            {
                uint numChildren = reader.ReadUInt32();
                long blockSize = ReadBracket(reader, owner);
                children = new osg_Geode[numChildren];
                for (uint i = 0; i < numChildren; ++i)
                {
                    children[i] = LoadObject(reader, owner) as osg_Geode;  //Geode 
                }
            }
        }
    }
}
