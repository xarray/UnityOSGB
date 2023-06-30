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
    public class osgMono_PagedLOD : osgMono_Base
    {
        private osg_PagedLOD m_osgPagedLOD;
        public osg_PagedLOD osgPagedLOD { get => m_osgPagedLOD; set { m_osgPagedLOD = value; Generate(); } }
        /// <summary> LOD 范围模式 距离/视图范围 </summary>
        public RangeMode rangeMode { get => osgPagedLOD.rangeMode; }
        /// <summary> 本文件路径 </summary>
        public string filePath { get => osgPagedLOD.owner.filePath; }
        /// <summary> 本文件名 </summary>
        public string fileName { get => Path.GetFileName(osgPagedLOD.owner.filePath); }
        /// <summary> 本文件所在目录 </summary>
        public string fileDirectory { get => filePath.Remove(filePath.Length - fileName.Length); }
        /// <summary> 子节点的相对路径 </summary>
        public string databasePath { get => osgPagedLOD.databasePath; }
        /// <summary> 子节点加载时的路径 </summary>
        public string fullPathPrefix { get => fileDirectory + databasePath; }
        /// <summary> 包围盒大小 </summary>
        public BoundingSphere bounds { get => osgPagedLOD.userCenter.Value; }
        /// <summary> 子节点的文件名 </summary>
        public string[] rangeData { get => osgPagedLOD.rangeData; }
        /// <summary> 子节点的LOD区间范围 </summary>
        public Vector2[] ranges { get => osgPagedLOD.ranges; }
        /// <summary> 子节点 </summary>
        public osgMono_LoadHelper[] childrens;
        /// <summary> 自身网格相关 </summary>
        public osgMono_Geode[] monoGeodes;
        private bool updated;
        private void Generate()
        {
            monoGeodes = new osgMono_Geode[osgPagedLOD.children.Length];
            childrens = new osgMono_LoadHelper[rangeData.Length];
            for (int i = 0; i < osgPagedLOD.children.Length; i++)
            {
                GameObject geodeGameObject = new GameObject();
                geodeGameObject.name = "Geode_" + i.ToString();
                geodeGameObject.transform.SetParent(transform);
                geodeGameObject.transform.localPosition = Vector3.zero;
                geodeGameObject.transform.localRotation = Quaternion.identity;
                geodeGameObject.transform.localScale = Vector3.one;
                var monoGeode = geodeGameObject.AddComponent<osgMono_Geode>();
                monoGeode.osgGeode = osgPagedLOD.children[i];
                monoGeodes[i] = monoGeode;
            }
            for (int i = 0; i < rangeData.Length; i++)
            {
                GameObject pagedNodeGameObject = new GameObject();
                pagedNodeGameObject.name = "PagedNode_" + i.ToString();
                pagedNodeGameObject.transform.SetParent(transform);
                pagedNodeGameObject.transform.localPosition = Vector3.zero;
                pagedNodeGameObject.transform.localRotation = Quaternion.identity;
                pagedNodeGameObject.transform.localScale = Vector3.one;
                childrens[i] = pagedNodeGameObject.AddComponent<osgMono_LoadHelper>();
                var fileName = rangeData[i];
                childrens[i].filePath = string.IsNullOrWhiteSpace(fileName) ? null : Path.Combine(fullPathPrefix, rangeData[i]);
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
                        if (childrens[i].Load())
                        {
                            foreach (var item in monoGeodes)
                            {
                                item.gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        if (childrens[i].UnLoad())
                        {
                            foreach (var item in monoGeodes)
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
