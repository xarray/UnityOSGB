using System.IO;
using UnityEngine;
namespace osgEx
{
    public class osg_Node : osg_Object
    { 
        public bool cullingActive;
        public int nodeMask; 
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            bool hasInitBound = reader.ReadBoolean();
            if (hasInitBound)
            {
                long blockSize = ReadBracket(reader, owner);
                Vector3 boudCenter = new Vector3(
                    (float)reader.ReadDouble(), (float)reader.ReadDouble(), (float)reader.ReadDouble());
                double radius = reader.ReadDouble();
            }
            bool hasComputeBoundCB = reader.ReadBoolean();
            if (hasComputeBoundCB) LoadObject(reader, owner);
           
            bool hasUpdateCB = reader.ReadBoolean();
            if (hasUpdateCB) LoadObject(reader, owner);
            
            bool hasEventCB = reader.ReadBoolean();
            if (hasEventCB) LoadObject(reader, owner);
           
            bool hasCullCB = reader.ReadBoolean();
            if (hasCullCB) LoadObject(reader, owner);
           
            cullingActive = reader.ReadBoolean();
            nodeMask = reader.ReadInt32();

            bool hasStateSet = reader.ReadBoolean();
            if (hasStateSet) LoadObject(reader, owner);
          
        }
    }
}
