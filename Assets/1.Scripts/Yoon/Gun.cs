using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {

    public Transform muzzle;
    //public Projectile projectile;

    public float shootInterval;
    public float muzzleVelocity;
    float nextShootTime;

    public Transform shell;
    public Transform shellEjection;
    MuzzleFlash muzzleflash;

    public float force;

    UIManger UIMgr;
    EventManager eventMgr;

    Vector3 mousePos;
    public Transform pos;
    private RaycastHit hit;
    private float range = 500;
    public GameObject crossHair;
    public GameObject shootEffect;

    public Camera viewCamera;


    public int gundamage = 1;
    public float hitForce = 100f;

    void Awake()
    {
        muzzleflash = GetComponent<MuzzleFlash>();
        viewCamera = Camera.main;
        //viewCamera = GetComponent<Camera>();
        if (viewCamera != null)
            Debug.Log("Success");
    }

    void Start()
    {
        muzzleVelocity = 35;
        shootInterval = 100;
        force = 300f;
    }

    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        if(Time.time > nextShootTime)
        {
            nextShootTime = Time.time + shootInterval / 1000;

            //Vector3 crossHairScreenPos = UIMgr.camUI.WorldToScreenPoint(UIMgr.goCrossHairs[0].transform.position);
            //Ray ray = viewCamera.ScreenPointToRay(crossHairScreenPos);
            Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green, 10f);
                if (hit.collider.gameObject.tag == "ENEMY")
                {
                    Instantiate(shootEffect, hit.point, Quaternion.identity);
                    Enemy health = hit.collider.GetComponent<Enemy>();
                    if (health != null)
                    {
                        health.Damage(gundamage);
                    }
                    // hit.rigidbody.AddForce(-hit.normal * hitForce);
                }
            }

            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleflash.Activate();
        }
    }
}
