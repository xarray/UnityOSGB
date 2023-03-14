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
    public class PagedNode : MonoBehaviour
    {
        public string fileName;
        public string fullPathPrefix; 
        public GameObject pagedNode;
        Coroutine GetCorutine; 
        public bool LoadPaged()
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            } 
            if (pagedNode)
            {
                return true;
            } 
            else
            {
                if (GetCorutine == null)
                {
                    GetCorutine = StartCoroutine(coroutine_loading()); 
                }
                return false;
            }
        }
        IEnumerator coroutine_loading()
        {
            var op = osg_Reader.CreateFromWebRequest(getFullFileName());
            yield return op;
            if (op.reader!=null)
            {
                pagedNode = osgHelper.CreateGameObject(op.reader, gameObject);
            }
            else
            {
                pagedNode = new GameObject();
            } 
            GetCorutine = null;
            yield break;
        }
 
        public bool UnLoadPaged()
        {
            if (!pagedNode)
            {
                if (GetCorutine!=null)
                {
                    StopCoroutine(GetCorutine);
                    GetCorutine = null;
                } 
            } 
            Destroy(pagedNode);
            return true;
        }
        public string getFullFileName()
        { return fullPathPrefix + fileName; }
    }
}
