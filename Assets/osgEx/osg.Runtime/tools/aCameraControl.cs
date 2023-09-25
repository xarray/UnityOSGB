using UnityEngine;

namespace osgEx.Tools
{
    public delegate void JudgeControl(ref bool bol);
    public abstract class aCameraController : MonoBehaviour
    {
        public static event JudgeControl judgeControl;
        public bool canControl
        {
            get
            {
                bool bol = true;
                if (judgeControl != null && judgeControl.GetInvocationList().Length != 0)
                {
                    judgeControl(ref bol);
                }
                return bol;
            }
        }

    }
}
