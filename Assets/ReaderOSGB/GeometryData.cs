using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace osgEx
{
    public class GeometryData : MonoBehaviour
    {
        public int _mode, _maxIndex = 0;
        public List<Vector2> _vec2Array;
        public List<Vector3> _vec3Array;
        public List<Vector4> _vec4Array;
        public List<Color> _vec4ubArray;
        public List<int> _indices = new List<int>();

        public void addPrimitiveIndices(List<int> localIndices)
        {
            switch (_mode)
            {
                case 4:  // TRIANGLES
                    _indices.AddRange(localIndices);
                    break;
                case 5:  // TRIANGLE_STRIP
                    for (int i = 2; i < localIndices.Count; ++i)
                    {
                        if ((i % 2) == 0)
                        {
                            _indices.Add(localIndices[i - 2]);
                            _indices.Add(localIndices[i - 1]);
                        }
                        else
                        {
                            _indices.Add(localIndices[i - 1]);
                            _indices.Add(localIndices[i - 2]);
                        }
                        _indices.Add(localIndices[i]);
                    }
                    break;
                case 6:  // TRIANGLE_FAN
                    for (int i = 2; i < localIndices.Count; ++i)
                    {
                        _indices.Add(localIndices[0]);
                        _indices.Add(localIndices[i - 1]);
                        _indices.Add(localIndices[i]);
                    }
                    break;
                default:
                    Debug.LogWarning("Unsupported primitive mode " + _mode);
                    break;
            }
        }
    }
}
