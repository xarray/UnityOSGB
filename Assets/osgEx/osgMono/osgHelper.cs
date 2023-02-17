using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using UnityEngine;
using UnityEngine.Pool; 

namespace osgEx
{
    public static class osgHelper
    {
        public static GameObject CreateGameObject(osg_Reader osgReader, GameObject parent = null)
        {
            if (osgReader == null)
            {
                throw new System.ArgumentNullException();
            }
            GameObject obj = new GameObject();
            obj.name = "OSGB_" + Path.GetFileName(osgReader.filePath);
            if (parent != null)
            {
                obj.transform.parent = parent.transform;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
            }
            int i = 0;
            if (osgReader.mainNode is osg_Group)
            {
                var group = osgReader.mainNode as osg_Group;
                foreach (var item in group.children)
                {
                    GameObject children = new GameObject();
                    children.name = "PagedLOD" + i;
                    children.transform.parent = obj.transform;
                    children.transform.localPosition = Vector3.zero;
                    children.transform.localRotation = Quaternion.identity;
                    children.transform.localScale = Vector3.one;
                    children.AddComponent<PagedLOD>().SetPagedLOD(item);
                    i++;
                }
                if (osgReader.mainNode is osg_MatrixTransform)
                {
                    var osgMatrix = osgReader.mainNode as osg_MatrixTransform;
                    obj.transform.localPosition = osgMatrix.localPosition;
                    obj.transform.localRotation = osgMatrix.localRotation;
                    obj.transform.localScale = osgMatrix.localScale;
                }
            }
            else if (osgReader.mainNode is osg_PagedLOD)
            {
                obj.AddComponent<PagedLOD>().SetPagedLOD(osgReader.mainNode as osg_PagedLOD);
            }

            return obj;
        }

       
    }
}
