using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Geode : osg_Node
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            GameObject parentObj = gameObj as GameObject;
            if (!base.read(gameObj, reader, owner))
                return false;

            bool hasChildren = reader.ReadBoolean();  // _drawables
            if (hasChildren)
            {
                uint numChildren = reader.ReadUInt32();
                long blockSize = ReadBracket(reader, owner);
                for (uint i = 0; i < numChildren; ++i)
                {
                    GameObject drawable = new GameObject("Drawable_" + i.ToString());
                    if (parentObj && LoadObject(drawable, reader, owner))
                        drawable.transform.SetParent(parentObj.transform, false);
                }
            }
            return true;
        }
    }
}
