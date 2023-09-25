using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_StateSet : osg_Object
    {

        void readModes(BinaryReader reader, osg_Reader owner, bool asTexMode)
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
        void readAttributes(BinaryReader reader, osg_Reader owner, bool asTexAttr)
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
                        if (asTexAttr)
                        {
                            osg_Texture temp = LoadObject(reader, owner) as osg_Texture;
                            textures ??= new List<osg_Texture>();
                            textures.Add(temp);
                        }
                        else
                        {
                            osg_Material temp = LoadObject(reader, owner) as osg_Material;
                            materials ??= new List<osg_Material>();
                            materials.Add(temp);
                        } 
                        int value = reader.ReadInt32(); 
                    }
                }
            }
        }
        
        public List<osg_Material> materials;
        public List<osg_Texture> textures;
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner); 

            bool hasModes = reader.ReadBoolean();  // _modeList
            if (hasModes) readModes(reader, owner, false);

            bool hasAttrs = reader.ReadBoolean();  // _attributeList
            if (hasAttrs) readAttributes(reader, owner, false);

            hasModes = reader.ReadBoolean();  // _textureModeList
            if (hasModes) readModes(reader, owner, true);

            hasAttrs = reader.ReadBoolean();  // _textureAttributeList
            if (hasAttrs) readAttributes(reader, owner, true);

            bool hasUniforms = reader.ReadBoolean();  // _uniformList
            if (hasUniforms)
            {
                int numUniforms = reader.ReadInt32();
                long blockSize = ReadBracket(reader, owner);
                for (int i = 0; i < numUniforms; ++i)
                {
                    LoadObject(reader, owner);
                    int value = reader.ReadInt32();
                }
            }

            int renderingHint = reader.ReadInt32();  // _renderingHint
            int binMode = reader.ReadInt32();  // _binMode
            int binNumber = reader.ReadInt32();  // _binNum
            string binName = ReadString(reader);  // _binName
            bool nestedBin = reader.ReadBoolean();  // _nestRenderBins

            bool hasUpdateCB = reader.ReadBoolean();  // _updateCallback
            if (hasUpdateCB) LoadObject(reader, owner);

            bool hasEventCB = reader.ReadBoolean();  // _eventCallback
            if (hasEventCB) LoadObject(reader, owner);

            if (owner._version >= 151)
            {
                bool hasDefListData = reader.ReadBoolean();  // _defineList
                if (hasDefListData)
                {
                    Debug.LogWarning("_defineList not implemented");
                }
            }
        }
    }
}
