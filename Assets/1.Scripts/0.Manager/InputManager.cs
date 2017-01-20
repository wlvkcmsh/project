using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

    Vector3 mousePos;
    public Transform pos;
    private RaycastHit hit;
    private float range = 500;
    public GameObject crossHair;
    public Camera mainCamera;

    public GameObject shootEffect;

    public Transform des; //총알 방향
    public float fireInterval;
    private float fireTime;
    public GameObject Bullet;
    public Transform Bullet_Emitter;
    public float Bullet_Forward_Force;
    private int Bullet_Damage;


    public float force;
    EventManager eventMgr;
    UIManger UIMgr;
    

    private static InputManager s_Instance = null;
    public static InputManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindObjectOfType(typeof(InputManager)) as InputManager;
            }
            return s_Instance;
        }
    }

    void Awake()
    {
        if (s_Instance != null)
        {
            Debug.LogError("Cannot have two instances of InputManager.");
            return;
        }
        s_Instance = this;
        Debug.Log("InputManager Awake");
    }
    void Start()
    {
        eventMgr = EventManager.Instance;
        UIMgr = UIManger.Instance;

        fireInterval = 0.3f;
        fireTime = 0.0f;
        Bullet_Emitter = eventMgr.curEvent.CurCamera.transform.GetChild(1).GetComponent<Transform>();
        Bullet_Forward_Force = 100.0f;

        force = 300f;
    }

    // Update is called once per frame
    void Update()
    {
        fireTime += Time.deltaTime;
        
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Shoot!!!");
            PressedShoot();
        }
    }
    
    void PressedShoot()
    {

        mainCamera = eventMgr.curEvent.CurCamera.transform.GetChild(1).GetComponent<Camera>();


        /*if (fireTime >= fireInterval)
        {

            fireTime = 0.0f;
        }*/

        ///총알
        Bullet_Emitter.position = mainCamera.transform.position;
        Bullet_Emitter.rotation = mainCamera.transform.rotation;

        

        ////////////

        Vector3 crossHairScreenPos = UIMgr.camUI.WorldToScreenPoint(UIMgr.goCrossHairs[0].transform.position);
        Ray ray = mainCamera.ScreenPointToRay(crossHairScreenPos);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.Log("나가라레이캐스트");
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green, 10f);

            GameObject Temporary_Bullet_Handler;

            Temporary_Bullet_Handler = Instantiate(Bullet, mainCamera.transform.position, Bullet_Emitter.transform.rotation) as GameObject;
            //Temporary_Bullet_Handler.transform.Rotate(Vector3.forward * 270);
            Rigidbody Tempory_RigidBody;
            Tempory_RigidBody = Temporary_Bullet_Handler.GetComponent<Rigidbody>();
            Tempory_RigidBody.AddForce(ray.direction * Bullet_Forward_Force);

            Destroy(Temporary_Bullet_Handler, 4.0f);

            if (hit.collider.gameObject.tag == "ENEMY")
            {
                hit.rigidbody.AddForceAtPosition(ray.direction* force, hit.point);
                //Instantiate(shootEffect, hit.point, Quaternion.identity);
                Debug.Log(hit.point);
            }
        }

    }
}
