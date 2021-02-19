using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMove : MonoBehaviour
{
    public float MRotationSpeed ;
    public float MMoveSpeed ;
    public float keySpeed ;

    private Vector3 rotaVector3;

    private float xspeed = -2f;//旋转速度
    private float yspeed = 4f;

    private double lastKickTime; // 上一次鼠标抬起的时间（用来处理双击）
    public bool _cameraRotation = false;

    public float moveSpeed = 150; // 设置相机移动速度    

    public bool isOperationCamera = true;//摄像机移动开关
    private Vector3 currentVelocity;

    private void Start()
    {
        MRotationSpeed = sliderValue(PlayerPrefs.GetFloat("MRotationSpeed", 100));
        MMoveSpeed = sliderValue(PlayerPrefs.GetFloat("MMoveSpeed", 100));
        keySpeed = sliderValue(PlayerPrefs.GetFloat("keySpeed", 100));

        rotaVector3 = transform.localEulerAngles;
        lastKickTime = Time.realtimeSinceStartup;

    }
    void Update()
    {
        if (isOperationCamera)
        {
            //旋转
            //if (Input.GetMouseButton(1))
            {
                if (Input.GetKey(KeyCode.W)) { transform.Translate(Vector3.forward * 5*keySpeed); }
                if (Input.GetKey(KeyCode.A)) { transform.Translate(Vector3.left * 5 * keySpeed); }
                if (Input.GetKey(KeyCode.S)) { transform.Translate(Vector3.back * 5 * keySpeed); }
                if (Input.GetKey(KeyCode.D)) { transform.Translate(Vector3.right * 5 * keySpeed); }
                if (Input.GetKey(KeyCode.Q)) { transform.Translate(Vector3.up * 5 * keySpeed); }
                if (Input.GetKey(KeyCode.E)) { transform.Translate(Vector3.down * 5 * keySpeed); }
            }
            //Vector3 oldCameraPosition = transform.position;
            // 当按住鼠标中键的时候    
            if (Input.GetMouseButton(2))
            {
                MoveCamera();
            }
            else if (Input.GetMouseButton(0))
            {
                float y = Input.GetAxis("Mouse X") * xspeed;
                float x = Input.GetAxis("Mouse Y") * xspeed;

                this.transform.Rotate(new Vector3(x,y,0));

                Vector3 startPos = transform.position;


            }
            
            MouseZoomCamera();

        }
    }
   
    public void MouseZoomCamera()
    {
        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (wheel > 0.25f|| wheel < -0.25f)
        {
            wheel *= Time.deltaTime * 500*MMoveSpeed;
        }
        else
        {
            wheel *= Time.deltaTime *100 * MMoveSpeed;
        }
        
        //改变相机的位置
        transform.Translate(Vector3.forward * wheel);

    }
    public void MoveCamera()
    {
        float h = Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime/2.0f*0.5f;
        float v = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime/2.0f*0.5f;
        // 设置当前摄像机移动，z轴并不改变    
        this.transform.Translate(-h, -v, 0, Space.Self);
    }

    public void stopMove()
    {
        _cameraRotation = false;
    }
    public float sliderValue(float m_slider)
    {
        float Num = 1;
        if (m_slider <= 100)
        {
            Num = (((float)m_slider) / 100);
        }
        else if (m_slider > 100)
        {
            Num = ((int)m_slider - 100) / 2;
        }
        if (Num == 0)
        {
            Num = 1;
        }
        return Num;
    }
  
}