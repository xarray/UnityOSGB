using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
            var op = osg_Reader.CreateFromWebRequest(filePath);
            yield return op;
            if (op.osgReader != null)
            {
                loadedGameObject = op.osgReader.CreateGameObject(gameObject);
            }
            else
            {
                loadedGameObject = new GameObject();
            }
            m_loadCorutine = null;
            yield break;
        }
    }
}
