using System.Collections;
using System.IO;
using UnityEngine;
using static UnityEditor.Progress;

namespace osgEx
{
    public class osgMono_LoadHelper : osgMono_Base
    {
        public string filePath;
        public GameObject loadedGameObject;
        Coroutine m_loadCorutine;
        public bool Load()
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                if (loadedGameObject)
                {
                    return true;
                }
                else if (m_loadCorutine == null)
                {
                    m_loadCorutine = StartCoroutine(coroutine_loading());
                }
            }
            return false;
        }
        public bool UnLoad()
        {
            if (!loadedGameObject)
            {
                if (m_loadCorutine != null)
                {
                    StopCoroutine(m_loadCorutine);
                    m_loadCorutine = null;
                }
            }
            Destroy(loadedGameObject);
            loadedGameObject = null;
            return true;
        }
        IEnumerator coroutine_loading()
        {
            gameObject.name = "loading_" + Path.GetFileName(filePath);
            var op = osg_Reader.LoadFromWebRequest(filePath);
            yield return op;
            if (op.osgReader != null)
            {
                gameObject.name = "loaded_" + Path.GetFileName(filePath);
                loadedGameObject = CreateGameObject(op.osgReader, gameObject);
            }
            else
            {
                gameObject.name = "error_" + Path.GetFileName(filePath);
                loadedGameObject = new GameObject();
                loadedGameObject.transform.parent = transform;
            }
            m_loadCorutine = null;
            yield break;
        }
        public static GameObject CreateGameObject(osg_Reader osgReader, GameObject parent = null)
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
