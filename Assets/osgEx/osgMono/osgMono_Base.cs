using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks; 
using UnityEngine;

namespace osgEx
{
    public class osgMono_Base :MonoBehaviour 
    {
        
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
