using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static osgEx.osg_LOD;

namespace osgEx
{
    /// <summary>
    /// OSGB 自带LOD信息 处理
    /// </summary>
    public class PagedLOD : MonoBehaviour
    {
        public RangeMode rangeMode = RangeMode.Distance;
        public string rootPath, databasePath, fullPathPrefix;
        public BoundingSphere bounds;
        public string[] rangeData;
        public Vector2[] ranges;
        public PagedNode[] pagedNodes;
        public Geode[] geodes;
        private bool updated;

        public void SetPagedLOD(osg_PagedLOD pagedLOD)
        {
            geodes = new Geode[pagedLOD.children.Length];
            databasePath = pagedLOD.databasePath;
            string fileName = Path.GetFileName(pagedLOD.owner.filePath);

            fullPathPrefix = rootPath = pagedLOD.owner.filePath.Remove(pagedLOD.owner.filePath.Length - fileName.Length);//      Path.GetDirectoryName(pagedLOD.owner._fileName) + Path.DirectorySeparatorChar;

            if (databasePath?.Length > 0) fullPathPrefix += databasePath + Path.DirectorySeparatorChar;

            rangeMode = pagedLOD.rangeMode;
            ranges = pagedLOD.ranges;
            bounds = pagedLOD.userCenter.Value;
            rangeData = pagedLOD.rangeData;
            pagedNodes = new PagedNode[rangeData.Length];
            for (int i = 0; i < pagedLOD.children.Length; i++)
            {
                GameObject obj = new GameObject();
                obj.name = "Geode_" + i.ToString();
                obj.transform.SetParent(transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
                var geode = obj.AddComponent<Geode>();
                geode.SetGeode(pagedLOD.children[i]);
                geodes[i] = geode;
            }
            for (int i = 0; i < rangeData.Length; i++)
            {
                GameObject obj = new GameObject();
                obj.name = "PagedNode_" + i.ToString();
                obj.transform.SetParent(transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
                pagedNodes[i] = obj.AddComponent<PagedNode>();
                pagedNodes[i].fileName = rangeData[i];
                pagedNodes[i].fullPathPrefix = fullPathPrefix;
            }
        }
         
        private void Update()
        {
            if (!osgManager.Instance.CanUpdatePaged)
            {
                updated = false;
                return;
            }
            if (!updated)
            { 
                Camera mainCam = Camera.main;

                Plane[] _currentFrustum = osgManager.Instance.CalculateFrustumPlanes; 

                Matrix4x4 local2World = this.transform.localToWorldMatrix; //* world2LocalOwner;

                Vector3 cameraPos = mainCam.transform.position;//); world2LocalOwner.MultiplyPoint(

                // Check LOD situation
                float rangeValue = 0.0f;
                if (rangeMode == RangeMode.Distance)
                {
                    Vector3 centerW = local2World.MultiplyPoint(bounds.position);
                    rangeValue = (cameraPos - centerW).magnitude;
                }
                else
                {
                    Vector3 centerW = local2World.MultiplyPoint(bounds.position);
                    Bounds bb = new Bounds(
                        centerW, new Vector3(bounds.radius, bounds.radius, bounds.radius) * 2.0f);
                    if (GeometryUtility.TestPlanesAABB(_currentFrustum, bb))
                    {
                        float distance = (centerW - cameraPos).magnitude;
                        float slope = Mathf.Tan(mainCam.fieldOfView * Mathf.Deg2Rad * 0.5f);
                        float projFactor = (0.5f * mainCam.pixelHeight) / (slope * distance);
                        rangeValue = bounds.radius * projFactor;  // screenPixelRadius
                    }
                    else
                        rangeValue = -1.0f;
                }
                for (int i = 0; i < ranges.Length; ++i)
                {
                    string fileName = rangeData[i];
                    if (fileName.Length == 0) continue;

                    Vector2 range = ranges[i];
                    if (range[0] < rangeValue && rangeValue < range[1])
                    {
                        if (pagedNodes[i].LoadPaged())
                        {
                            foreach (var item in geodes)
                            {
                                item.gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        if (pagedNodes[i].UnLoadPaged())
                        {
                            foreach (var item in geodes)
                            {
                                item.gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }  
        }


    }
}
