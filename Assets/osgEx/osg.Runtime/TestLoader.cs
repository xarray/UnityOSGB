
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace osgEx
{


    public class TestLoader : MonoBehaviour
    {
        public string __rootPath;
        private void Start()
        {
            if (string.IsNullOrEmpty(__rootPath))
            {
                return;
            }
            var dir = new DirectoryInfo(__rootPath);
            var filePaths = dir.GetDirectories().Select(x => x.Name + "/" + x.Name + ".osgb").ToArray();

            osgManager.Instance.LoadOSGB(__rootPath, filePaths);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
