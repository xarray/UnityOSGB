using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Node : osg_Object
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            bool hasInitBound = reader.ReadBoolean();  // _initialBound
            if (hasInitBound)
            {
                long blockSize = ReadBracket(reader, owner);
                Vector3 boudCenter = new Vector3(
                    (float)reader.ReadDouble(), (float)reader.ReadDouble(), (float)reader.ReadDouble());
                double radius = reader.ReadDouble();
            }

            bool hasComputeBoundCB = reader.ReadBoolean();  // _computeBoundCallback
            if (hasComputeBoundCB) LoadObject(gameObj, reader, owner);

            bool hasUpdateCB = reader.ReadBoolean();  // _updateCallback
            if (hasUpdateCB) LoadObject(gameObj, reader, owner);

            bool hasEventCB = reader.ReadBoolean();  // _eventCallback
            if (hasEventCB) LoadObject(gameObj, reader, owner);

            bool hasCullCB = reader.ReadBoolean();  // _cullCallback
            if (hasCullCB) LoadObject(gameObj, reader, owner);

            bool cullingActive = reader.ReadBoolean();  // _cullingActive
            int nodeMask = reader.ReadInt32();  // _nodeMask

            bool hasStateSet = reader.ReadBoolean();  // _stateset
            if (hasStateSet) LoadObject(gameObj, reader, owner);

            return true;
        }
    }
}
