using System;
using UnityEngine;

namespace osgEx.Tools
{

    public class VirtualCameraController : aCameraController
    {
        [Serializable]
        public struct ControlData
        {
            public float moveSpeed;
            public float rotateSpeed;
            public float rotateAroundSpeed;
        }

        [SerializeField]
        private ControlData m_data;

#if ENABLE_INPUT_SYSTEM
        [SerializeField]
        private InputActionMap m_map;
        private InputAction m_zoomAction;
        private InputAction m_dragPositionAction;
        private InputAction m_dragButtonAction;
        private InputAction m_moveAction;
        private InputAction m_rotateButtonAction;
        private InputAction m_rotateDeltaAction;
        private InputAction m_rotateAroundButtonAction;
        //缩进值
        private float zoomValue { get => m_zoomAction.ReadValue<Vector2>().y * 0.02f; }
         
        //拖拽位置
        private Vector2 dragPositionValue { get => m_dragPositionAction.ReadValue<Vector2>(); }
        //是否处于拖拽控制中
        private bool isDragging { get => m_dragButtonAction.IsPressed(); } 
        //移动值
        private Vector3 moveValue { get => m_moveAction.ReadValue<Vector3>(); }
        //旋转增量
        private Vector2 rotateDeltaValue { get => m_rotateDeltaAction.ReadValue<Vector2>(); }
        //是否处于旋转控制中
        private bool isRotating { get => m_rotateButtonAction.IsPressed(); }
        private bool isRotateAround { get => m_rotateAroundButtonAction.IsPressed(); }
#else
        //缩进值
        private float zoomValue { get => Input.GetAxis("Mouse ScrollWheel") * 60; } 
        //拖拽位置
        private Vector2 dragPositionValue { get => Input.mousePosition; }
        //是否处于拖拽控制中
        private bool isDragging { get => Input.GetMouseButton(0); }
        //移动值
        private Vector3 moveValue { get => new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")); }
        //旋转增量
        private Vector2 rotateDeltaValue { get => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); }
        //是否处于旋转控制中
        private bool isRotating { get => Input.GetMouseButton(1); }
        private bool isRotateAround { get => Input.GetMouseButton(1) && Input.GetKey(KeyCode.LeftControl); }

#endif

        //拖拽点
        Vector3? m_dragTargetPosition;
        //绕此点旋转
        Vector3? m_rotateAroundPosition;

        //是否已有移动相关的控制,避免控制冲突
        bool positionControl;
        //是否已有旋转相关的控制,避免控制冲突
        bool rotationControl;
        private void Awake()
        {
#if Input
            m_cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
            m_cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
            m_dragButtonAction = m_map.FindAction("dragButton"); 
            m_dragPositionAction = m_map.FindAction("dragPosition"); 
            m_zoomAction = m_map.FindAction("zoom"); 
            m_moveAction = m_map.FindAction("move"); 
            m_rotateButtonAction = m_map.FindAction("rotateButton"); 
            m_rotateDeltaAction = m_map.FindAction("rotateDelta"); 
            m_rotateAroundButtonAction = m_map.FindAction("rotateAroundButton"); 
            m_map.Enable();
#else
#endif

        }
        /// <summary>
        /// 拖拽控制
        /// </summary>
        /// <returns>是否处于控制状态</returns>
        void DragControl()
        {
            if (isDragging && !positionControl)
            {
                Vector2 value = dragPositionValue;
                Ray ray = Camera.main.ScreenPointToRay(value);
                if (m_dragTargetPosition == null)
                {
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        m_dragTargetPosition = hit.point;
                    }
                }
                if (m_dragTargetPosition != null)
                {
                    Plane plane = new Plane(Camera.main.transform.forward * -1, (Vector3)m_dragTargetPosition);
                    if (plane.Raycast(ray, out float dis))
                    {
                        Vector3 rayPlaneCast = ray.GetPoint(dis);
                        Vector3 position = (Vector3)m_dragTargetPosition - rayPlaneCast + Camera.main.transform.position;
                        transform.position = position;
                    }
                    positionControl = true;
                }
            }
            else
            {
                m_dragTargetPosition = null;
            }
        }
        /// <summary>
        /// 缩进控制
        /// </summary>
        /// <returns>是否处于控制状态</returns>
        void ZoomControl()
        {
            float value = zoomValue;
            if (value != 0 && !positionControl)
            {
                Ray ray = new Ray(transform.position, transform.forward);
                float distance = 20;
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    distance = hit.distance < 20 ? 20 : hit.distance;
                }
                Vector3 position = value * transform.forward * distance * Time.fixedDeltaTime + transform.position;
                transform.position = position;
                positionControl = true;
            }
        }
        /// <summary>
        /// 移动控制
        /// </summary>
        /// <returns>是否处于控制状态</returns>
        void MoveControl()
        {
            Vector3 value = moveValue;
            if (value != Vector3.zero && !positionControl)
            {
                value *= Time.fixedDeltaTime * m_data.moveSpeed;
                //左右
                Vector3 offset = transform.right * value.x;
                //前后
                offset += transform.forward * value.z;
                //上下
                //offset.y += value.y;
                transform.position = transform.position + offset;
                positionControl = true;
            }
        }
        /// <summary>
        /// 旋转控制
        /// </summary>
        /// <returns>是否处于控制状态</returns>
        void RotateControl()
        {
            Vector2 value = rotateDeltaValue;
            //判断状态
            if (isRotating && !rotationControl)
            {
                if (value != Vector2.zero)
                {
                    if (isRotateAround && !positionControl)
                    {
                        if (m_rotateAroundPosition == null)
                        {
                            Ray ray = new Ray(transform.position, transform.forward);
                            if (Physics.Raycast(ray, out RaycastHit raycastHit))
                            {
                                m_rotateAroundPosition = raycastHit.point;
                            }
                        }
                        if (m_rotateAroundPosition != null)
                        {
                            value *= m_data.rotateAroundSpeed * Time.fixedDeltaTime;
                            transform.RotateAround((Vector3)m_rotateAroundPosition, Vector3.up, value.x);
                            Quaternion valueQuaternion = Quaternion.AngleAxis(value.y, transform.right * -1);

                            Vector3 vector2 = transform.position - (Vector3)m_rotateAroundPosition;
                            vector2 = valueQuaternion * vector2;
                            Vector3 vector3 = (Vector3)m_rotateAroundPosition + vector2;
                            float angle = Vector3.Angle(vector2, Vector3.up);
                            if (angle > 5 && angle < 175)
                            {
                                transform.position = vector3;
                                transform.rotation = Quaternion.LookRotation(vector2 * -1);
                            }
                            positionControl = true;
                            rotationControl = true;
                        }
                        return;
                    }
                    else
                    {
                        value *= m_data.rotateSpeed * Time.fixedDeltaTime;
                        //旋转差值
                        Quaternion diffQuaternion = Quaternion.Euler(-value.y, value.x, 0);
                        //旋转结果
                        Quaternion valueQuaternion = transform.localRotation;
                        valueQuaternion *= diffQuaternion;
                        //计算X轴角度
                        Vector3 forward = valueQuaternion * Vector3.forward;
                        float angle = Vector3.Angle(forward, Vector3.up);
                        //不符合角度返回
                        if (angle > 175 || angle < 5)
                        {
                            return;
                        }
                        //z轴值归0
                        Vector3 eulerAngles = valueQuaternion.eulerAngles;
                        eulerAngles.z = 0;
                        valueQuaternion.eulerAngles = eulerAngles;
                        transform.localRotation = valueQuaternion;
                        rotationControl = true;
                        m_rotateAroundPosition = null;
                        return;
                    }
                }
                return;
            }
            m_rotateAroundPosition = null;
        }

        // Update is called once per frame
        void Update()
        {

            // if (canControl && m_cinemachineBrain.IsLive(m_cinemachineVirtualCamera))
            {
                positionControl = false;
                rotationControl = false;
                DragControl();
                RotateControl();
                ZoomControl();
                MoveControl();
            }

        }

    }
}
