using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_Object : ObjectBase
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            string name = ReadString(reader);  // _name
            int dataVariance = reader.ReadInt32();  // _dataVariance

            bool hasUserData = reader.ReadBoolean();  // _userData
            if (hasUserData)
            {
                Debug.LogWarning("_userData not implemented");
                return false;
            }

            if (name.Length > 0) gameObj.name = name;
            else if (gameObj.name.Contains("Child"))
            {
                string objName = gameObj.name;
                gameObj.name = objName.Replace("Child", this.GetType().Name);
            }
            return true;
        }
    }
}
