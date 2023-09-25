using System.IO;

namespace osgEx
{
    class osg_UserDataContainer : osg_Object
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
        }
    }
    class osg_DefaultUserDataContainer : osg_UserDataContainer
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            bool hasUserData = reader.ReadBoolean();
            if (hasUserData)
            {
                long blockSize = ReadBracket(reader, owner);
                var userData = LoadObject(reader, owner);
                // reader.BaseStream.Position += blockSize;
            }
            bool hasDescriptions = reader.ReadBoolean();
            if (hasDescriptions)
            {
                int size = reader.ReadInt32();
                long blockSize = ReadBracket(reader, owner);
                for (int i = 0; i < size; i++)
                {
                    var description = ReadString(reader);
                }
            }
            bool hasUserObjects = reader.ReadBoolean();
            if (hasUserObjects)
            {
                int size = reader.ReadInt32();
                long blockSize = ReadBracket(reader, owner);
                for (int i = 0; i < size; i++)
                {
                    var userObject = LoadObject(reader, owner);
                }
            }
        }
    }
  
}
