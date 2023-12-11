using UnityEngine;

namespace osgEx
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class osgMono_Geometry : osgMono_Base<osg_Geometry>
    {
        private MeshRenderer m_meshRenderer;
        public MeshRenderer meshRenderer { get { if (m_meshRenderer == null) { m_meshRenderer = this.GetOrAddComponent<MeshRenderer>(); } return m_meshRenderer; } }
        private MeshFilter m_meshFilter;
        public MeshFilter meshFilter { get { if (m_meshFilter == null) { m_meshFilter = this.GetOrAddComponent<MeshFilter>(); } return m_meshFilter; } }
        private MeshCollider m_meshCollider;
        public MeshCollider meshCollider { get { if (m_meshCollider == null) { m_meshCollider = this.GetOrAddComponent<MeshCollider>(); } return m_meshCollider; } }
        private Mesh m_mesh;
        private Texture2D m_mainTexture;
        public override void Generate(osg_Geometry osgGeometry)
        {
            meshRenderer.material = osgManager.Instance.materialData.Material;
            if (meshRenderer.material != null)
            {
                var m_materialPropertyBlock = new MaterialPropertyBlock();
                m_mainTexture = (osgGeometry.stateSet?.textures?[0] as osg_Texture2D).Generate();
                if (!string.IsNullOrWhiteSpace(osgManager.Instance.materialData.MainTexProperty))
                {
                    m_materialPropertyBlock.SetTexture(osgManager.Instance.materialData.MainTexProperty, m_mainTexture);
                }
                var emission = osgGeometry.stateSet?.materials?[0].emission;
                var diffuse = osgGeometry.stateSet?.materials?[0].diffuse;
                var ambient = osgGeometry.stateSet?.materials?[0].ambient;
                var specular = osgGeometry.stateSet?.materials?[0].specular;
                meshRenderer.SetPropertyBlock(m_materialPropertyBlock);
            }
            if (m_mesh != null) { Destroy(m_mesh); m_mesh = null; }
            m_mesh = new Mesh();
            m_mesh.vertices = osgGeometry.vertexs;
            m_mesh.triangles = osgGeometry.indices;
            m_mesh.uv = osgGeometry.uv[0];
            meshFilter.sharedMesh = m_mesh;
            meshCollider.sharedMesh = m_mesh;
            meshCollider.enabled = osgManager.Instance.colliderEnabled;

            if (osgGeometry.normals != null)
            {
                m_mesh.normals = osgGeometry.normals;
            }
            else
            {
                m_mesh.RecalculateNormals();
            }
            m_mesh.UploadMeshData(true);
        }

        private void OnDestroy()
        {
            if (m_mesh != null)
            {
                Destroy(m_mesh);
            }
            if (m_mainTexture != null)
            {
                Destroy(m_mainTexture);
            }
        }
    }
}
