using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Group : osg_Node
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            GameObject parentObj = gameObj as GameObject;
            if (!base.read(gameObj, reader, owner))
                return false;

            bool hasChildren = reader.ReadBoolean();  // _children
            if (hasChildren)
            {
                uint numChildren = reader.ReadUInt32();
                long blockSize = ReadBracket(reader, owner);
                for (uint i = 0; i < numChildren; ++i)
                {
                    GameObject children = new GameObject("Child_" + i.ToString());
                    if (parentObj && LoadObject(children, reader, owner))
                        children.transform.SetParent(parentObj.transform, false);
                }
            }
            return true;
        }
    }
}
