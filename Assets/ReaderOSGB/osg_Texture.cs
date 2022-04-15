using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Texture : osg_StateAttribute
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            if (!base.read(gameObj, reader, owner))
                return false;

            bool hasWrap = reader.ReadBoolean();  // _wrap_s
            if (hasWrap) { int wrapS = reader.ReadInt32(); }

            hasWrap = reader.ReadBoolean();  // _wrap_r
            if (hasWrap) { int wrapR = reader.ReadInt32(); }

            hasWrap = reader.ReadBoolean();  // _wrap_t
            if (hasWrap) { int wrapT = reader.ReadInt32(); }

            bool hasFilter = reader.ReadBoolean(); // _min_filter
            if (hasFilter) { int minFilter = reader.ReadInt32(); }

            hasFilter = reader.ReadBoolean(); // _mag_filter
            if (hasFilter) { int magFilter = reader.ReadInt32(); }

            float maxAnisotropy = reader.ReadSingle();  // _maxAnisotropy
            bool useHardwareMipmap = reader.ReadBoolean();  // _useHardwareMipMapGeneration
            bool unrefImageAfterApply = reader.ReadBoolean();  // _unrefImageDataAfterApply
            bool clientStorageHint = reader.ReadBoolean();  // _clientStorageHint
            bool resizeNPOT = reader.ReadBoolean();  // _resizeNonPowerOfTwoHint
            Vector4 borderColor = new Vector4(  // _borderColor
                (float)reader.ReadDouble(), (float)reader.ReadDouble(),
                (float)reader.ReadDouble(), (float)reader.ReadDouble());
            int borderWidth = reader.ReadInt32();  // _borderWidth
            int formatMode = reader.ReadInt32();  // _internalFormatMode

            bool hasInternalFormat = reader.ReadBoolean(); // _internalFormat
            if (hasInternalFormat) { int internalFormat = reader.ReadInt32(); }

            bool hasSourceFormat = reader.ReadBoolean(); // _sourceFormat
            if (hasSourceFormat) { int sourceFormat = reader.ReadInt32(); }

            bool hasSourceType = reader.ReadBoolean(); // _sourceType
            if (hasSourceType) { int sourceType = reader.ReadInt32(); }

            bool useShadowCompare = reader.ReadBoolean();  // _use_shadow_comparison
            int shadowCompareFunc = reader.ReadInt32();  // _shadow_compare_func
            int shadowTextureMode = reader.ReadInt32();  // _shadow_texture_mode
            float shadowAmbient = reader.ReadSingle();  // _shadow_ambient

            if (owner._version >= 95 && owner._version < 153)
            {
                bool hasImageAttachment = reader.ReadBoolean(); // _imageAttachment
                if (hasImageAttachment)
                {
                    int unit = reader.ReadInt32();
                    int level = reader.ReadInt32();
                    bool layered = reader.ReadBoolean();
                    int layer = reader.ReadInt32();
                    int access = reader.ReadInt32();
                    int format = reader.ReadInt32();
                }
            }

            if (owner._version >= 98)
            {
                bool hasSwizzle = reader.ReadBoolean(); // _swizzle
                if (hasSwizzle) { string swizzle = ReadString(reader); }
            }

            if (owner._version >= 155)
            {
                float minLOD = reader.ReadSingle();  // _minLOD
                float maxLOD = reader.ReadSingle();  // _maxLOD
                float lodBias = reader.ReadSingle();  // _lodBias
            }
            return true;
        }
    }
}
