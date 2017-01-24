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

    private Gun gun;

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

        gun = GetComponent<Gun>();
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
            gun.Shoot();
        }
    }
}
