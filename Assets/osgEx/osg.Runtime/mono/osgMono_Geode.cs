using System;
using UnityEngine;

namespace osgEx
{
    public class osgMono_Geode : osgMono_Base<osg_Geode>
    {
        public osgMono_Geometry[] monoGeometrys;
        public override void Generate(osg_Geode osgGeode)
        {
            if (osgGeode.geometrys == null)
            {
                return;
            }
            monoGeometrys = new osgMono_Geometry[osgGeode.geometrys.Length];
            for (int i = 0; i < osgGeode.geometrys.Length; i++)
            {
                var geometry = osgGeode.geometrys[i];
                GameObject geometryGameObject = new GameObject();
                geometryGameObject.name = "Geometry_" + i.ToString();
                geometryGameObject.transform.SetParent(transform);
                geometryGameObject.transform.localPosition = Vector3.zero;
                geometryGameObject.transform.localRotation = Quaternion.identity;
                geometryGameObject.transform.localScale = Vector3.one;
                monoGeometrys[i] = geometryGameObject.AddComponent<osgMono_Geometry>();
                monoGeometrys[i].Generate(geometry);
            }
        }
    }
}
