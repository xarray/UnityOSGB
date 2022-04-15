using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_DrawArrays : osg_PrimitiveSet
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            GameObject parentObj = gameObj as GameObject;
            if (!base.read(gameObj, reader, owner))
                return false;

            int first = reader.ReadInt32();  // First
            int count = reader.ReadInt32();  // Count

            GeometryData gd = parentObj.GetComponent<GeometryData>();
            if (gd != null)
            {
                List<int> localIndices = new List<int>();
                for (int i = 0; i < count; ++i)
                {
                    int id = first + i; localIndices.Add(id);
                    if (gd._maxIndex < id) gd._maxIndex = id;
                }
                gd.addPrimitiveIndices(localIndices);
            }
            return true;
        }
    }
}
