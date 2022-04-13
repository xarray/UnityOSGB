using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class osg_Geometry : osg_Drawable
{
    public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
    {
        GameObject parentObj = gameObj as GameObject;
        if (!base.read(gameObj, reader, owner))
            return false;

        Mesh mesh = new Mesh();
        mesh.name = parentObj.name + "_Mesh";

        // _primitives
        uint numPrimitives = reader.ReadUInt32();
        GeometryData gd = parentObj.AddComponent<GeometryData>();
        for (uint i = 0; i < numPrimitives; ++i) LoadObject(parentObj, reader, owner);

        // just a temperatory method to enable triangles applied
        mesh.vertices = new Vector3[gd._maxIndex + 1];
        mesh.triangles = gd._indices.ToArray();
        Debug.Log(gd._maxIndex + ": " + mesh.triangles.Length);

        bool hasData = reader.ReadBoolean();  // _vertexData
        if (hasData) LoadObject(parentObj, reader, owner);
        hasData = reader.ReadBoolean();  // _normalData
        if (hasData) LoadObject(parentObj, reader, owner);
        hasData = reader.ReadBoolean();  // _colorData
        if (hasData) LoadObject(parentObj, reader, owner);
        hasData = reader.ReadBoolean();  // _secondaryColorData
        if (hasData) LoadObject(parentObj, reader, owner);
        hasData = reader.ReadBoolean();  // _fogCoordData
        if (hasData) LoadObject(parentObj, reader, owner);

        uint numArrayData = reader.ReadUInt32();  // _texCoordList
        for (uint i = 0; i < numArrayData; ++i) LoadObject(parentObj, reader, owner);

        numArrayData = reader.ReadUInt32();  // _vertexAttribList
        for (uint i = 0; i < numArrayData; ++i) LoadObject(parentObj, reader, owner);
        
        bool hasFastPath = reader.ReadBoolean();  // _fastPathHint
        if (hasFastPath) { bool fastPathHint = reader.ReadBoolean(); }

        // Mesh related components
        mesh.RecalculateBounds();
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
