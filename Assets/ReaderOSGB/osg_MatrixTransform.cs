using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_MatrixTransform : osg_Transform
    {
        public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 forward, upwards;
            forward.x = matrix.m02; forward.y = matrix.m12; forward.z = matrix.m22;
            upwards.x = matrix.m01; upwards.y = matrix.m11; upwards.z = matrix.m21;
            return Quaternion.LookRotation(forward, upwards);
        }

        public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            GameObject parentObj = gameObj as GameObject;
            if (!base.read(gameObj, reader, owner))
                return false;

            // _matrix
            long blockSize = ReadBracket(reader, owner);

            Matrix4x4 matrix = new Matrix4x4();
            matrix.m00 = (float)reader.ReadDouble(); matrix.m10 = (float)reader.ReadDouble();
            matrix.m20 = (float)reader.ReadDouble(); matrix.m30 = (float)reader.ReadDouble();
            matrix.m01 = (float)reader.ReadDouble(); matrix.m11 = (float)reader.ReadDouble();
            matrix.m21 = (float)reader.ReadDouble(); matrix.m31 = (float)reader.ReadDouble();
            matrix.m02 = (float)reader.ReadDouble(); matrix.m12 = (float)reader.ReadDouble();
            matrix.m22 = (float)reader.ReadDouble(); matrix.m32 = (float)reader.ReadDouble();
            matrix.m03 = (float)reader.ReadDouble(); matrix.m13 = (float)reader.ReadDouble();
            matrix.m23 = (float)reader.ReadDouble(); matrix.m33 = (float)reader.ReadDouble();

            parentObj.transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
            parentObj.transform.localRotation = ExtractRotationFromMatrix(ref matrix);
            parentObj.transform.localScale = ExtractScaleFromMatrix(ref matrix);
            return true;
        }
    }
}
