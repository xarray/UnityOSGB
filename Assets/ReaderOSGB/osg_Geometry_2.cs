using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Geometry_2 : osg_Drawable_2
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            GameObject parentObj = gameObj as GameObject;
            if (!base.read(gameObj, reader, owner))
                return false;

            Mesh mesh = new Mesh();
            mesh.name = parentObj.name + "_Mesh";

            // _primitives
            uint numPrimitives = reader.ReadUInt32(), maxIndex = 0;
            GeometryData gd = parentObj.AddComponent<GeometryData>();
            for (uint i = 0; i < numPrimitives; ++i)
            {
                if (owner._version < 112)
                {
                    uint id = GeometryUtils.readPrimitive(ref gd._indices, reader, owner);
                    if (maxIndex < id) maxIndex = id;
                }
                else
                    LoadObject(parentObj, reader, owner);
            }

            // just a temperatory method to enable triangles applied
            mesh.vertices = new Vector3[maxIndex + 1];
            mesh.triangles = gd._indices.ToArray();

            bool hasNormals = false;
            if (owner._version < 112)
            {
                bool hasArrayData = reader.ReadBoolean();  // _vertexData
                if (hasArrayData) GeometryUtils.readArrayData("vertex", mesh, reader, owner);

                hasArrayData = reader.ReadBoolean();  // _normalData
                if (hasArrayData)
                {
                    GeometryUtils.readArrayData("normal", mesh, reader, owner);
                    hasNormals = true;
                }

                hasArrayData = reader.ReadBoolean();  // _colorData
                if (hasArrayData) GeometryUtils.readArrayData("color", mesh, reader, owner);

                hasArrayData = reader.ReadBoolean();  // _secondaryColorData
                if (hasArrayData) GeometryUtils.readArrayData("secondaryColor", mesh, reader, owner);

                hasArrayData = reader.ReadBoolean();  // _fogCoordData
                if (hasArrayData) GeometryUtils.readArrayData("fogCoord", mesh, reader, owner);

                hasArrayData = reader.ReadBoolean();  // _texCoordList
                if (hasArrayData) GeometryUtils.readArrayListData("texCoord", mesh, reader, owner);

                hasArrayData = reader.ReadBoolean();  // _vertexAttribList
                if (hasArrayData) GeometryUtils.readArrayListData("vertexAttrib", mesh, reader, owner);

                bool hasFastPath = reader.ReadBoolean();  // _fastPathHint
                if (hasFastPath) { }  // { bool fastPathHint = reader.ReadBoolean(); }
            }
            else
            {
                bool hasData = reader.ReadBoolean();  // _vertexData
                if (hasData)
                {
                    if (LoadObject(parentObj, reader, owner))
                        mesh.vertices = gd._vec3Array.ToArray();
                }

                hasData = reader.ReadBoolean();  // _normalData
                if (hasData)
                {
                    if (LoadObject(parentObj, reader, owner))
                    { mesh.normals = gd._vec3Array.ToArray(); hasNormals = true; }
                }

                hasData = reader.ReadBoolean();  // _colorData
                if (hasData) LoadObject(parentObj, reader, owner);
                hasData = reader.ReadBoolean();  // _secondaryColorData
                if (hasData) LoadObject(parentObj, reader, owner);
                hasData = reader.ReadBoolean();  // _fogCoordData
                if (hasData) LoadObject(parentObj, reader, owner);

                uint numArrayData = reader.ReadUInt32();  // _texCoordList
                for (uint i = 0; i < numArrayData; ++i)
                {
                    if (LoadObject(parentObj, reader, owner))
                    {
                        if (i == 0) mesh.uv = gd._vec2Array.ToArray();
                        else mesh.uv2 = gd._vec2Array.ToArray();
                    }
                }

                numArrayData = reader.ReadUInt32();  // _vertexAttribList
                for (uint i = 0; i < numArrayData; ++i) LoadObject(parentObj, reader, owner);
            }

            // Mesh related components
            if (!hasNormals) mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            Object.Destroy(gd);
            
            parentObj.AddComponent<MeshFilter>().sharedMesh = mesh;
            if (mesh.vertexCount > 3)
            {
                MeshCollider collider = parentObj.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;
                collider.convex = true;
            }

            MeshRenderer renderer = parentObj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = GameObject.Instantiate<Material>(owner._template);
            renderer.sharedMaterial.mainTexture = owner._preloadedTexture;
            return true;
        }
    }
}
