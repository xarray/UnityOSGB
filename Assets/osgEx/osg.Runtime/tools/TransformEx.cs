using System;
using System.Collections;
using UnityEngine;

namespace osgEx
{
    public class TransformEx : MonoBehaviour
    {
        public Action onChanged;
        public bool hasChanged { get; private set; }
        Transform thisTransform;
        private void Awake()
        {
            this.hideFlags = HideFlags.HideInInspector;
            this.thisTransform = transform;
        }
        private void Update()
        {
            hasChanged = thisTransform.hasChanged;
            if (hasChanged)
            {
                onChanged?.Invoke();
                thisTransform.hasChanged = false;
            } 
        }
        IEnumerator EndOfF()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

            }
        }
        private void LateUpdate()
        {

        }
    }
}
