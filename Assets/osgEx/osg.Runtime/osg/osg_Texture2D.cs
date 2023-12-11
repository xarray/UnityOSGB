using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Texture2D : osg_Texture
    {
        private int width;
        private int height;
        private string fileName;
        private byte[] rawTextureData;
        private byte[] imageData;
        private uint id;
        private TextureFormat format;
        private int decision;

        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            bool hasImage = reader.ReadBoolean();  // _image
            if (hasImage)
                readImage(reader, owner);

            int texWidth = reader.ReadInt32();
            int texHeight = reader.ReadInt32();
        }
        protected void readImage(BinaryReader reader, osg_Reader owner)
        {
            if (owner._version > 94)
            {
                string className = ReadString(reader);
            }
            id = reader.ReadUInt32();
            if (owner._sharedObjects.TryGetValue(id, out osg_Object other))
            {
                fileName = ((osg_Texture2D)other).fileName;
                width = ((osg_Texture2D)other).width;
                height = ((osg_Texture2D)other).height;
                rawTextureData = ((osg_Texture2D)other).rawTextureData;
                imageData = ((osg_Texture2D)other).imageData;
                format = ((osg_Texture2D)other).format;
                decision = ((osg_Texture2D)other).decision;
                return;
            }
            owner._sharedObjects.Add(id, this);

            fileName = ReadString(reader);
            int writeHint = reader.ReadInt32();
            decision = reader.ReadInt32();
            switch (decision)
            {
                case 0:  // IMAGE_INLINE_DATA
                    {
                        int origin = reader.ReadInt32();
                        int s = reader.ReadInt32();
                        width = reader.ReadInt32();
                        height = reader.ReadInt32();
                        int internalFormat = reader.ReadInt32();
                        int pixelFormat = reader.ReadInt32();
                        int dataType = reader.ReadInt32();
                        int packing = reader.ReadInt32();
                        int mode = reader.ReadInt32();

                        uint size = reader.ReadUInt32();
                        rawTextureData = reader.ReadBytes((int)size);
                        if (size > 0)
                        {
                            format = TextureFormat.RGB24;  // TODO: other formats/data size
                            if (dataType == 0x1401)
                            {
                                if (pixelFormat == 0x1907) format = TextureFormat.RGB24;  // GL_RGB
                                else if (pixelFormat == 0x1908) format = TextureFormat.RGBA32;  // GL_RGBA
                                else if (pixelFormat == 0x80E1) format = TextureFormat.BGRA32;  // GL_BGRA
                                else if (pixelFormat == 0x83F0)
                                    format = TextureFormat.DXT1;  // GL_COMPRESSED_RGB_S3TC_DXT1_EXT
                                else if (pixelFormat == 0x83F3)
                                    format = TextureFormat.DXT5;  // GL_COMPRESSED_RGB_S3TC_DXT5_EXT
                                else Debug.LogWarning("Unsupported texture pixel format " + pixelFormat);
                            }
                            else
                                Debug.LogWarning("Unsupported texture data type " + dataType);
                        }

                        uint numLevels = reader.ReadUInt32();
                        for (uint i = 0; i < numLevels; ++i)
                        {
                            uint levelDataSize = reader.ReadUInt32();
                            // TODO
                        }
                    }
                    break;
                case 1:  // IMAGE_INLINE_FILE
                    {
                        uint size = reader.ReadUInt32();
                        if (size > 0)
                        {
                            imageData = reader.ReadBytes((int)size);
                        }
                    }
                    break;
                case 2:  // IMAGE_EXTERNAL
                    if (File.Exists(fileName))
                    {

                    }
                    break;
                default: break;
            }

            var name = ReadString(reader);  // _name
            var dataVariance = reader.ReadInt32();  // _dataVariance 
            bool hasUserData = reader.ReadBoolean();  // _userData
            if (hasUserData)
            {
                var userData = LoadObject(reader, owner);
            }
        }

        public Texture2D Generate()
        { 
            Texture2D m_texture2D = null;
            switch (decision)
            {
                case 0:
                    m_texture2D = new Texture2D(width, height, format, false);
                    m_texture2D.LoadRawTextureData(rawTextureData);
                    m_texture2D.Apply(false, true);
                    break;
                case 1:
                    m_texture2D = new Texture2D(2, 2);
                    m_texture2D.LoadImage(imageData, true);
                    break;
                case 2:
                    imageData = File.ReadAllBytes(fileName);
                    m_texture2D.LoadImage(imageData, true);
                    break;
                default:
                    break;
            }
            return m_texture2D;
        }

    }
}
