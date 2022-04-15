using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Texture2D : osg_Texture
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            bool hasImage = reader.ReadBoolean();  // _image
            if (hasImage)
                owner._preloadedTexture = LoadImage(gameObj, reader, owner);

            int texWidth = reader.ReadInt32();
            int texHeight = reader.ReadInt32();
            return true;
        }
    }
}
