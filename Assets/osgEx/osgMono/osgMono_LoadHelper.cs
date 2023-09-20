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
                loadedGameObject = op.osgReader.CreateGameObject(gameObject);
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
    }
}
