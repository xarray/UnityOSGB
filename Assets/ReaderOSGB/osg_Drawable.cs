using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Drawable : osg_Node
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            bool hasInitBound = reader.ReadBoolean();  // _initialBound
            if (hasInitBound)
            {
                long blockSize = ReadBracket(reader, owner);
                Vector3 boundMin = new Vector3(
                    (float)reader.ReadDouble(), (float)reader.ReadDouble(), (float)reader.ReadDouble());
                Vector3 boundMax = new Vector3(
                    (float)reader.ReadDouble(), (float)reader.ReadDouble(), (float)reader.ReadDouble());
            }

            bool hasComputeBoundCB = reader.ReadBoolean();  // _computeBoundCallback
            if (hasComputeBoundCB) LoadObject(gameObj, reader, owner);

            bool hasShape = reader.ReadBoolean();  // _shape
            if (hasShape) LoadObject(gameObj, reader, owner);

            bool enableDisplaylists = reader.ReadBoolean();  // _supportsDisplayList
            bool useDisplaylists = reader.ReadBoolean();  // _useDisplayList
            bool useVBO = reader.ReadBoolean();  // _useVertexBufferObjects

            if (owner._version >= 142)
            {
                int nodeMask = reader.ReadInt32();  // _nodeMask
            }

            if (owner._version >= 145)
            {
                bool active = reader.ReadBoolean();  // _cullingActive
            }
            return true;
        }
    }
}
