using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_LOD : osg_Node
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            GameObject parentObj = gameObj as GameObject;
            if (!base.read(gameObj, reader, owner))
                return false;

            PagedData pagedData = parentObj.AddComponent<PagedData>();
            pagedData._mainReader = owner;

            int centerMode = reader.ReadInt32();  // _centerMode

            bool hasUserCenter = reader.ReadBoolean();  // _userDefinedCenter, _radius
            if (hasUserCenter)
            {
                double centerX = reader.ReadDouble();
                double centerY = reader.ReadDouble();
                double centerZ = reader.ReadDouble();
                double radius = reader.ReadDouble();

                pagedData._bounds = new BoundingSphere(
                    new Vector3((float)centerX, (float)centerY, (float)centerZ), (float)radius);
            }
            // TODO: _bounds in default center mode?

            int rangeMode = reader.ReadInt32();  // _rangeMode
            pagedData._rangeMode = (rangeMode > 0)
                                 ? PagedData.RangeMode.PixelSize : PagedData.RangeMode.Distance;

            bool hasRanges = reader.ReadBoolean();  // _rangeList
            if (hasRanges)
            {
                uint numRanges = reader.ReadUInt32();
                long blockSize = ReadBracket(reader, owner);
                for (uint i = 0; i < numRanges; ++i)
                {
                    float minR = reader.ReadSingle();
                    float maxR = reader.ReadSingle();
                    pagedData._ranges.Add(new Vector2(minR, maxR));
                }
            }

            return true;
        }
    }
}
