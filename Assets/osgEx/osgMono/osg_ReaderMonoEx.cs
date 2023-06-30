using System.IO; 
using UnityEngine; 

namespace osgEx
{
    public static class osg_ReaderMonoEx
    {
        public static GameObject CreateGameObject(this osg_Reader osgReader, GameObject parent = null)
        {
            if (osgReader == null)
            {
                throw new System.ArgumentNullException();
            }
            GameObject osgbGameObject = new GameObject();
            osgbGameObject.name = "OSGB_" + Path.GetFileName(osgReader.filePath);
            if (parent != null)
            {
                osgbGameObject.transform.parent = parent.transform;
                osgbGameObject.transform.localPosition = Vector3.zero;
                osgbGameObject.transform.localRotation = Quaternion.identity;
                osgbGameObject.transform.localScale = Vector3.one;
            }

            if (osgReader.root is osg_Group group)
            {
                for (int i = 0; i < group.children.Length; i++)
                {
                    var item = group.children[i];
                    GameObject children = new GameObject();
                    children.name = "PagedLOD_" + i;
                    children.transform.parent = osgbGameObject.transform;
                    children.transform.localPosition = Vector3.zero;
                    children.transform.localRotation = Quaternion.identity;
                    children.transform.localScale = Vector3.one;
                    children.AddComponent<osgMono_PagedLOD>().osgPagedLOD = item;
                }
                if (osgReader.root is osg_MatrixTransform matrix)
                {
                    osgbGameObject.transform.localPosition = matrix.localPosition;
                    osgbGameObject.transform.localRotation = matrix.localRotation;
                    osgbGameObject.transform.localScale = matrix.localScale;
                }
            }
            else if (osgReader.root is osg_PagedLOD pagedLOD)
            {
                osgbGameObject.AddComponent<osgMono_PagedLOD>().osgPagedLOD = pagedLOD;
            }

            return osgbGameObject;
        }


    }
}
