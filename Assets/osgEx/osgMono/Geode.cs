using System;
using UnityEngine;

namespace osgEx
{
    public class OnDestroyEvent : MonoBehaviour
    {
        public event Action OnDestroyEvt;
        private void Awake()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }
        private void OnDestroy()
        {
            if (OnDestroyEvt != null)
            {
                OnDestroyEvt();
            }
        }
    }
    public class Geode : MonoBehaviour
    {
        public void SetGeode(osg_Geode geode)
        {
            if (geode.drawables == null)
            {
                return;
            }
            for (int i = 0; i < geode.drawables.Length; i++)
            {
                var drawable = geode.drawables[i];
                GameObject drawableObj = new GameObject();

                drawableObj.name = "Drawable_" + i.ToString();
                drawableObj.transform.SetParent(transform);
                drawableObj.transform.localPosition = Vector3.zero;
                drawableObj.transform.localRotation = Quaternion.identity;
                drawableObj.transform.localScale = Vector3.one;
                MeshRenderer renderer = drawableObj.AddComponent<MeshRenderer>();
                MeshFilter filter = drawableObj.AddComponent<MeshFilter>();

                renderer.material = UnityEngine.Object.Instantiate(osgManager.Instance.templeteMaterial);

                renderer.material.mainTexture = (drawable.stateSet?.textures?[0] as osg_Texture2D).texture2D;

                Mesh mesh = new Mesh();
                mesh.vertices = drawable.vertexs;
                mesh.triangles = drawable.indices;
                mesh.uv = drawable.uv[0];
                filter.sharedMesh = mesh;
                if (drawable.normals != null)
                {
                    mesh.normals = drawable.normals;
                }
                else
                {
                    mesh.RecalculateNormals();
                }
                drawableObj.GetOrAddComponent<OnDestroyEvent>().OnDestroyEvt += () =>
                {
                    Destroy(mesh);
                    Destroy(renderer.material);
                };
            }
        }
    }
}
