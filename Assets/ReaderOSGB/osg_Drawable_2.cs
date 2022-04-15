using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Drawable_2 : osg_Object
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            bool hasStateSet = reader.ReadBoolean();  // _stateset
            if (hasStateSet) LoadObject(gameObj, reader, owner);

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

            bool hasUpdateCB = reader.ReadBoolean();  // _updateCallback
            if (hasUpdateCB) LoadObject(gameObj, reader, owner);

            bool hasEventCB = reader.ReadBoolean();  // _eventCallback
            if (hasEventCB) LoadObject(gameObj, reader, owner);

            bool hasCullCB = reader.ReadBoolean();  // _cullCallback
            if (hasCullCB) LoadObject(gameObj, reader, owner);

            bool hasDrawCB = reader.ReadBoolean();  // _drawCallback
            if (hasDrawCB) LoadObject(gameObj, reader, owner);

            return true;
        }
    }
}
