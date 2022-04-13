using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class osg_PrimitiveSet : osg_BufferData  // FIXME: version >= 147
{
    public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
    {
        GameObject parentObj = gameObj as GameObject;
        if (!base.read(gameObj, reader, owner))
            return false;

        int numInstances = reader.ReadInt32();  // NumInstances
        int mode = reader.ReadInt32();  // Mode

        GeometryData gd = parentObj.GetComponent<GeometryData>();
        if (gd != null) gd._mode = mode;
        return true;
    }
}
