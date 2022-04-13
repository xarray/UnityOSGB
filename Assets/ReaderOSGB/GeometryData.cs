using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GeometryData : MonoBehaviour
{
    public int _mode, _maxIndex = 0;
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
