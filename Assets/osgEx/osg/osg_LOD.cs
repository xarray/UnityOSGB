using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_LOD : osg_Node
    {
        public enum RangeMode { Distance, PixelSize };

        public int centerMode;
        public BoundingSphere? userCenter;
        public RangeMode rangeMode;
        public Vector2[] ranges;
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);
            centerMode = reader.ReadInt32();  // _centerMode

            bool hasUserCenter = reader.ReadBoolean();  // _userDefinedCenter, _radius
            if (hasUserCenter)
            {
                double centerX = reader.ReadDouble();
                double centerY = reader.ReadDouble();
                double centerZ = reader.ReadDouble();
                double radius = reader.ReadDouble();

                userCenter = new BoundingSphere(
                    new Vector3((float)centerX, (float)centerY, (float)centerZ), (float)radius);
            }
            // TODO: _bounds in default center mode?

            // _rangeMode
            int rangeModeInt32 = reader.ReadInt32();
            rangeMode = (rangeModeInt32 > 0) ? RangeMode.PixelSize : RangeMode.Distance;
            // _rangeList
            bool hasRanges = reader.ReadBoolean();
            if (hasRanges)
            {
                uint numRanges = reader.ReadUInt32();
                long blockSize = ReadBracket(reader, owner);
                ranges = new Vector2[numRanges];
                for (uint i = 0; i < numRanges; ++i)
                {
                    float minR = reader.ReadSingle();
                    float maxR = reader.ReadSingle();
                    ranges[i] = new Vector2(minR, maxR);
                }
            }
        }

    }
}
