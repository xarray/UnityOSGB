using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using UnityEngine;

namespace osgEx
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class osgMono_Geometry : osgMono_Base
    {
        private osg_Geometry m_osgGeometry;
        public osg_Geometry osgGeometry { get => m_osgGeometry; set { m_osgGeometry = value; Generate(); } }
        private MeshRenderer m_meshRenderer;
        public MeshRenderer meshRenderer { get { if (m_meshRenderer == null) { m_meshRenderer = this.GetOrAddComponent<MeshRenderer>(); } return m_meshRenderer; } }
        private MeshFilter m_meshFilter;
        public MeshFilter meshFilter { get { if (m_meshFilter == null) { m_meshFilter = this.GetOrAddComponent<MeshFilter>(); } return m_meshFilter; } }

        public Mesh currentMesh { get; private set; }

        void Generate()
        {
            meshRenderer.material = osgManager.Instance.material;

            var m_materialPropertyBlock = new MaterialPropertyBlock();
            var texture = (osgGeometry.stateSet?.textures?[0] as osg_Texture2D).texture2D;
            m_materialPropertyBlock.SetTexture(osgManager.Instance.materialMainTexID, texture);
            meshRenderer.SetPropertyBlock(m_materialPropertyBlock);

            currentMesh = new Mesh();
            currentMesh.vertices = osgGeometry.vertexs;
            currentMesh.triangles = osgGeometry.indices;
            currentMesh.uv = osgGeometry.uv[0];
            meshFilter.sharedMesh = currentMesh;
            if (osgGeometry.normals != null)
            {
                currentMesh.normals = osgGeometry.normals;
            }
            else
            {
                currentMesh.RecalculateNormals();
            }
        }

        private void OnDestroy()
        {
            if (currentMesh != null)
            {
                Destroy(currentMesh);
            } 
        }
    }
}
