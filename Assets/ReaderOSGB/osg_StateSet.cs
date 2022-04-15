using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_StateSet : osg_Object
    {
        void readModes(Object gameObj, BinaryReader reader, ReaderOSGB owner, bool asTexMode)
        {
            int numTexUnits = asTexMode ? reader.ReadInt32() : 1;
            if (asTexMode) ReadBracket(reader, owner);

            for (int u = 0; u < numTexUnits; ++u)
            {
                int numModes = reader.ReadInt32();
                if (numModes > 0)
                {
                    long blockSize = ReadBracket(reader, owner);
                    for (int i = 0; i < numModes; ++i)
                    {
                        int glEnum = reader.ReadInt32();
                        int value = reader.ReadInt32();
                    }
                }
            }
        }

        void readAttributes(Object gameObj, BinaryReader reader, ReaderOSGB owner, bool asTexAttr)
        {
            int numTexUnits = asTexAttr ? reader.ReadInt32() : 1;
            if (asTexAttr) { long blockSize = ReadBracket(reader, owner); }

            for (int u = 0; u < numTexUnits; ++u)
            {
                int numAttrs = reader.ReadInt32();
                if (numAttrs > 0)
                {
                    long blockSize = ReadBracket(reader, owner);
                    for (int i = 0; i < numAttrs; ++i)
                    {
                        LoadObject(gameObj, reader, owner);
                        int value = reader.ReadInt32();
                    }
                }
            }
        }

        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            bool hasModes = reader.ReadBoolean();  // _modeList
            if (hasModes) readModes(gameObj, reader, owner, false);

            bool hasAttrs = reader.ReadBoolean();  // _attributeList
            if (hasAttrs) readAttributes(gameObj, reader, owner, false);

            hasModes = reader.ReadBoolean();  // _textureModeList
            if (hasModes) readModes(gameObj, reader, owner, true);

            hasAttrs = reader.ReadBoolean();  // _textureAttributeList
            if (hasAttrs) readAttributes(gameObj, reader, owner, true);

            bool hasUniforms = reader.ReadBoolean();  // _uniformList
            if (hasUniforms)
            {
                int numUniforms = reader.ReadInt32();
                long blockSize = ReadBracket(reader, owner);
                for (int i = 0; i < numUniforms; ++i)
                {
                    LoadObject(gameObj, reader, owner);
                    int value = reader.ReadInt32();
                }
            }

            int renderingHint = reader.ReadInt32();  // _renderingHint
            int binMode = reader.ReadInt32();  // _binMode
            int binNumber = reader.ReadInt32();  // _binNum
            string binName = ReadString(reader);  // _binName
            bool nestedBin = reader.ReadBoolean();  // _nestRenderBins

            bool hasUpdateCB = reader.ReadBoolean();  // _updateCallback
            if (hasUpdateCB) LoadObject(gameObj, reader, owner);

            bool hasEventCB = reader.ReadBoolean();  // _eventCallback
            if (hasEventCB) LoadObject(gameObj, reader, owner);

            if (owner._version >= 151)
            {
                bool hasDefListData = reader.ReadBoolean();  // _defineList
                if (hasDefListData)
                {
                    Debug.LogWarning("_defineList not implemented");
                    return false;
                }
            }
            return true;
        }
    }
}
