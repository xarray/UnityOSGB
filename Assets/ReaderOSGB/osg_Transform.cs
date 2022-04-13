using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class osg_Transform : osg_Group
{
    public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
    {
        GameObject parentObj = gameObj as GameObject;
        if (!base.read(gameObj, reader, owner))
            return false;

        int referenceFrame = reader.ReadInt32();  // _referenceFrame
        return true;
    }
}
