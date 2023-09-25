using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Texture2D : osg_Texture
    {
        public int width { get; private set; }
        public int height { get; private set; }
        public Texture2D texture2D;
        protected override void read(BinaryReader reader, osg_Reader owner)
        {
            base.read(reader, owner);

            bool hasImage = reader.ReadBoolean();  // _image
            if (hasImage)
                texture2D = LoadImage(reader, owner);

            int texWidth = reader.ReadInt32();
            int texHeight = reader.ReadInt32();
        }
        public static Texture2D LoadImage(BinaryReader reader, osg_Reader owner)
        {
            Texture2D tex2D = null;
            if (owner._version > 94)
            {
                string className = ReadString(reader);
            }
            uint id = reader.ReadUInt32();
            if (owner._sharedTextures.ContainsKey(id))
                return owner._sharedTextures[id];

            string fileName = ReadString(reader);
            int writeHint = reader.ReadInt32();
            int decision = reader.ReadInt32();
            switch (decision)
            {
                case 0:  // IMAGE_INLINE_DATA
                    {
                        int origin = reader.ReadInt32();
                        int s = reader.ReadInt32();
                        int t = reader.ReadInt32();
                        int r = reader.ReadInt32();
                        int internalFormat = reader.ReadInt32();
                        int pixelFormat = reader.ReadInt32();
                        int dataType = reader.ReadInt32();
                        int packing = reader.ReadInt32();
                        int mode = reader.ReadInt32();

                        uint size = reader.ReadUInt32();
                        byte[] imageData = reader.ReadBytes((int)size);
                        if (size > 0)
                        {
                            TextureFormat format = TextureFormat.RGB24;  // TODO: other formats/data size
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

                            tex2D = new Texture2D(s, t, format, false);
                            tex2D.LoadRawTextureData(imageData);
                            tex2D.Apply();
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
                            byte[] fileData = reader.ReadBytes((int)size);

                            tex2D = new Texture2D(2, 2);
                            tex2D.LoadImage(fileData);
                            //File.WriteAllBytes("test.jpg", fileData);
                        }
                    }
                    break;
                case 2:  // IMAGE_EXTERNAL
                    if (File.Exists(fileName))
                    {
                        byte[] fileData = File.ReadAllBytes(fileName);

                        tex2D = new Texture2D(2, 2);
                        tex2D.LoadImage(fileData);
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
            owner._sharedTextures[id] = tex2D;
            return tex2D;
        }
    }
}
