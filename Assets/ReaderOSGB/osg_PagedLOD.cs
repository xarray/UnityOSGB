using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class osg_PagedLOD : osg_LOD
    {
        public override bool read(Object gameObj, BinaryReader reader, ReaderOSGB owner)
        {
            GameObject parentObj = gameObj as GameObject;
            if (!base.read(gameObj, reader, owner))
                return false;

            PagedData pagedData = parentObj.GetComponent<PagedData>();
            pagedData._rootFileName = owner._currentFileName;

            bool hasPath = reader.ReadBoolean();  // _databasePath
            if (hasPath)
            {
                bool notEmptyPath = reader.ReadBoolean();
                if (notEmptyPath)
                {
                    string databasePath = ReadString(reader);
                    if (pagedData != null) pagedData._databasePath = databasePath;
                }
            }

            //int numFrames = reader.ReadInt32();  // _frameNumberOfLastTraversal
            int numExpired = reader.ReadInt32();  // _numChildrenThatCannotBeExpired
            bool disableExternalPaging = reader.ReadBoolean();  // _disableExternalChildrenPaging

            bool hasRangeData = reader.ReadBoolean();  // _perRangeDataList
            if (hasRangeData)
            {
                uint numRanges = reader.ReadUInt32();
                long blockSize = ReadBracket(reader, owner);
                for (uint i = 0; i < numRanges; ++i)
                {
                    string pagedFile = ReadString(reader);
                    //Debug.Log(i + ": " + pagedFile);
                    if (pagedData != null) pagedData._fileNames.Add(pagedFile);
                }

                uint numPriorityOffsets = reader.ReadUInt32();
                blockSize = ReadBracket(reader, owner);
                for (uint i = 0; i < numPriorityOffsets; ++i)
                {
                    float offset = reader.ReadSingle();
                    float scale = reader.ReadSingle();
                }
            }

            bool hasChildren = reader.ReadBoolean();  // _children (preloaded)
            if (hasChildren)
            {
                uint numChildren = reader.ReadUInt32();
                long blockSize = ReadBracket(reader, owner);
                for (uint i = 0; i < numChildren; ++i)
                {
                    GameObject children = new GameObject("Child_" + i.ToString());
                    if (parentObj && LoadObject(children, reader, owner))
                        children.transform.SetParent(parentObj.transform, false);
                    if (pagedData != null) pagedData._pagedNodes.Add(children);
                }
            }

            return true;
        }
    }
}
