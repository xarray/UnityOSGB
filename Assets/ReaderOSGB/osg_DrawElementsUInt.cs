using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_DrawElementsUInt : osg_PrimitiveSet
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            GameObject parentObj = gameObj as GameObject;
            if (!base.read(gameObj, reader, owner))
                return false;
            
            GeometryData gd = parentObj.GetComponent<GeometryData>();
            if (gd != null)
            {
                List<int> localIndices = new List<int>();
                int numElements = reader.ReadInt32();
                for (uint n = 0; n < numElements; ++n)
                {
                    uint value = reader.ReadUInt32(); localIndices.Add((int)value);
                    if (gd._maxIndex < (int)value) gd._maxIndex = (int)value;
                }
                gd.addPrimitiveIndices(localIndices);
            }
            return true;
        }
    }
}
