using UnityEngine;

namespace osgEx
{
    public class osgMono_Base : MonoBehaviour
    {

    }
    public abstract class osgMono_Base<T> : osgMono_Base
    {
        public abstract void Generate(T args); 
    }
    public static class osgMonoEx
    {
        public static void GetOrAddComponent<T>(this osgMono_Base osgMono, ref T field) where T : MonoBehaviour
        {

            if (field != null)
            {
                return;
            }
            else
            {
                field = osgMono.GetOrAddComponent<T>();
            }
        }
    }

}
