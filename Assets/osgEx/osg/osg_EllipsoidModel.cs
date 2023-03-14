using System.IO;

namespace osgEx
{
    public class osg_EllipsoidModel : osg_Object
    {
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            double radiusE = reader.ReadDouble();  // _radiusEquator
            double radiusP = reader.ReadDouble();  // _radiusPolar

        }
    }
}
