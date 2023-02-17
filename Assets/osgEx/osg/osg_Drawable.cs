using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Drawable : osg_Object
    {
        Vector3? boundMin;
        Vector3? boundMax;

        public osg_StateSet stateSet;
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            if (owner._version < 154)
            {
                bool hasStateSet = reader.ReadBoolean();  // _stateset
                if (hasStateSet)
                {
                    stateSet = LoadObject(reader, owner) as osg_StateSet;
                }
            }
            bool hasInitBound = reader.ReadBoolean();  // _initialBound
            if (hasInitBound)
            {
                long blockSize = ReadBracket(reader, owner);
                boundMin = new Vector3(
                  (float)reader.ReadDouble(), (float)reader.ReadDouble(), (float)reader.ReadDouble());
                boundMax = new Vector3(
                  (float)reader.ReadDouble(), (float)reader.ReadDouble(), (float)reader.ReadDouble());
            }
            bool hasComputeBoundCB = reader.ReadBoolean();
            if (hasComputeBoundCB) LoadObject(reader, owner);

            bool hasShape = reader.ReadBoolean();
            if (hasShape) LoadObject(reader, owner);

            bool enableDisplaylists = reader.ReadBoolean();  // _supportsDisplayList
            bool useDisplaylists = reader.ReadBoolean();  // _useDisplayList
            bool useVBO = reader.ReadBoolean();  // _useVertexBufferObjects 

            if (owner._version >= 154)
            {
                //if (owner._version >= 142)
                {
                    int nodeMask = reader.ReadInt32();  // _nodeMask
                }
                //if (owner._version >= 145)
                {
                    bool active = reader.ReadBoolean();  // _cullingActive
                }
            }
            else
            {
                bool hasUpdateCB = reader.ReadBoolean();  // _updateCallback
                if (hasUpdateCB) LoadObject(reader, owner);

                bool hasEventCB = reader.ReadBoolean();  // _eventCallback
                if (hasEventCB) LoadObject(reader, owner);

                bool hasCullCB = reader.ReadBoolean();  // _cullCallback
                if (hasCullCB) LoadObject(reader, owner);

                bool hasDrawCB = reader.ReadBoolean();  // _drawCallback
                if (hasDrawCB) LoadObject(reader, owner);
            }
        }
    }
}
