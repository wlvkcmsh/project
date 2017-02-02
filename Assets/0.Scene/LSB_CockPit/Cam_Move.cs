using UnityEngine;
using System.Collections;

public class Cam_Move : MonoBehaviour
{
    //public GameObject Leg; // 위치를 따라갈 물체 
    //public GameObject Leg;// 다리 기준점이 될 물체

    //float rotSpeed = 0.5f;

    //public float Max_Degree_X = 35; // 돌아갈 최대 각도
    //public float Max_Degree_Y = 20; // 돌아갈 최대 각도

    //public float Mouse_Move_Speed = 0.5f; // 마우스 움직임 속도(민감도)

    //public bool Move_x = true; // 움직임 허용 
    //public bool Move_y = true;

    //float L_Max = 0; // 제한각도
    //float R_Max = 0;

    //bool L_Over = false; // 넘어감 확인 변수
    //bool R_Over = false;

    public Vector3 v3Rotate;
   // public Vector3 CamPos = new Vector3(-1f,3f,0f);
    public float xmin = -20;
    public float xmax = 20;
    public float zmin = -10;
    public float zmax = 10;
    public float rotateSpeed = 45.0f;

    public float a = 0;
    public float b = 0;

    void Start()
    {
        transform.localEulerAngles = v3Rotate;
    }

    void Update()
    {
        //transform.position = Leg.transform.position + CamPos; // 위치 따라가기

        //v3Rotate.y += Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
        //v3Rotate.y = Mathf.Clamp(v3Rotate.y, xmin, xmax);
        //transform.localEulerAngles = v3Rotate;

        //v3Rotate.x -= Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
        //v3Rotate.x = Mathf.Clamp(v3Rotate.x, zmin, zmax);
        //transform.localEulerAngles = v3Rotate;

        a = Input.GetAxis("RightJoystickHorizontal");

        v3Rotate.y += Input.GetAxis("RightJoystickHorizontal") * rotateSpeed * Time.deltaTime;
        v3Rotate.y = Mathf.Clamp(v3Rotate.y, xmin, xmax);
        transform.localEulerAngles = v3Rotate;

        b = Input.GetAxis("RightJoystickVertical");

        v3Rotate.x += Input.GetAxis("RightJoystickVertical") * rotateSpeed * Time.deltaTime;
        v3Rotate.x = Mathf.Clamp(v3Rotate.x, zmin, zmax);
        transform.localEulerAngles = v3Rotate;

        //float MouseX = Input.GetAxis("Mouse X"); // 마우스 값 받아오기
        //float MouseY = Input.GetAxis("Mouse Y");
        //Vector3 temp = new Vector3(MouseY, MouseX,  0).normalized;
        //transform.Rotate(temp * 100 * Time.deltaTime);
        //transform.RotateAroundLocal(Vector3.up, MouseX * 10 * Time.deltaTime);
        //transform.RotateAroundLocal(transform.right, MouseY * 10 * Time.deltaTime);



        //if (Move_x == true)
        //{
        //    float MouseX = Input.GetAxis("Mouse X"); // 마우스 값 받아오기


        //    if (Leg.transform.rotation.eulerAngles.y - Max_Degree_X <= 0) // 다리 중심점으로 왼쪽 제한각도
        //    {
        //        L_Max = 360 - Mathf.Abs(Leg.transform.rotation.eulerAngles.y - Max_Degree_X);
        //        L_Over = true;
        //    }
        //    else
        //    {
        //        L_Max = Leg.transform.rotation.eulerAngles.y - Max_Degree_X;
        //        L_Over = false;
        //    }

        //    if (Leg.transform.rotation.eulerAngles.y + Max_Degree_X >= 360)// 다리 중심점으로 오른쪽 제한각도
        //    {
        //        R_Max = (Leg.transform.rotation.eulerAngles.y + Max_Degree_X) - 360;
        //        R_Over = true;
        //    }
        //    else
        //    {
        //        R_Max = Leg.transform.rotation.eulerAngles.y + Max_Degree_X;
        //        R_Over = false;
        //    }


        //    if (MouseX < 0) // 왼쪽으로 마우스 움직일때
        //    {
        //        if (L_Over == true || R_Over == true)
        //        {
        //            if (L_Max < transform.rotation.eulerAngles.y && 360 > transform.rotation.eulerAngles.y)
        //            {
        //                if (L_Max < transform.rotation.eulerAngles.y + -Mouse_Move_Speed)
        //                {
        //                    transform.Rotate(Vector3.up * rotSpeed * -Mouse_Move_Speed);
        //                }
        //            }
        //            if (0 <= transform.rotation.eulerAngles.y && transform.rotation.eulerAngles.y < R_Max)
        //            {
        //                if (R_Max > transform.rotation.eulerAngles.y + -Mouse_Move_Speed)
        //                {
        //                    transform.Rotate(Vector3.up * rotSpeed * -Mouse_Move_Speed);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (L_Max < transform.rotation.eulerAngles.y + -Mouse_Move_Speed)
        //            {
        //                transform.Rotate(Vector3.up * rotSpeed * -Mouse_Move_Speed);
        //            }
        //        }
        //    }
        //    if (MouseX > 0) // 오른쪽으로 마우스 움직일때
        //    {
        //        if (R_Over == true || L_Over == true)
        //        {
        //            if (0 <= transform.rotation.eulerAngles.y && transform.rotation.eulerAngles.y < R_Max)
        //            {
        //                if (R_Max > transform.rotation.eulerAngles.y + Mouse_Move_Speed)
        //                {
        //                    transform.Rotate(Vector3.up * rotSpeed * Mouse_Move_Speed);
        //                }
        //            }
        //            if (L_Max < transform.rotation.eulerAngles.y && transform.rotation.eulerAngles.y < 360)
        //            {
        //                if (L_Max < transform.rotation.eulerAngles.y + Mouse_Move_Speed)
        //                {
        //                    transform.Rotate(Vector3.up * rotSpeed * Mouse_Move_Speed);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (R_Max > transform.rotation.eulerAngles.y + Mouse_Move_Speed)
        //            {
        //                transform.Rotate(Vector3.up * rotSpeed * Mouse_Move_Speed);
        //            }
        //        }
        //    }
        //}

        //if (Move_y == true)
        //{
        //    float MouseY = Input.GetAxis("Mouse Y");
        //    if (MouseY > 0) // 위으로 마우스 움직일때 양수
        //    {
        //        if ((transform.rotation.eulerAngles.x - Mouse_Move_Speed) > (360 - Max_Degree_Y) || transform.rotation.eulerAngles.x == 0)
        //        {
        //            transform.Rotate(Vector3.left * rotSpeed * Mouse_Move_Speed);
        //        }
        //        if ((transform.rotation.eulerAngles.x - Mouse_Move_Speed) < (0 + Max_Degree_Y) || transform.rotation.eulerAngles.x == 0)
        //        {
        //            transform.Rotate(Vector3.left * rotSpeed * Mouse_Move_Speed);
        //        }
        //    }
        //    if (MouseY < 0) // 아래쪽으로 마우스 움직일때 음수
        //    {
        //        if ((transform.rotation.eulerAngles.x + Mathf.Abs(Mouse_Move_Speed)) < (0 + Max_Degree_Y) || transform.rotation.eulerAngles.x == 0)
        //        {
        //            transform.Rotate(Vector3.left * rotSpeed * -Mouse_Move_Speed);
        //        }
        //        if ((transform.rotation.eulerAngles.x - -Mouse_Move_Speed) > (360 - Max_Degree_Y) || transform.rotation.eulerAngles.x == 0)
        //        {
        //            transform.Rotate(Vector3.left * rotSpeed * -Mouse_Move_Speed);
        //        }
        //    }
        //}
    }
    
}
