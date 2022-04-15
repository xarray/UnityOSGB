using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Vec4ubArray : osg_Array
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            GameObject parentObj = gameObj as GameObject;
            if (!base.read(gameObj, reader, owner))
                return false;

            int numData = reader.ReadInt32();
            List<Color> vArray = new List<Color>();
            for (int i = 0; i < numData; ++i)
            {
                Color v = new Color(
                    (float)reader.ReadByte() / 255.0f, (float)reader.ReadByte() / 255.0f,
                    (float)reader.ReadByte() / 255.0f, (float)reader.ReadByte() / 255.0f);
                vArray.Add(v);
            }

            GeometryData gd = parentObj.GetComponent<GeometryData>();
            if (gd != null) gd._vec4ubArray = vArray;
            return true;
        }
    }
}
