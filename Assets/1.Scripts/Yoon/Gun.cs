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

    Ray check_ray;
    private float rayTime;
    public float interval;

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
    public Canvas canvas;

    public int gundamage = 1;
    public float hitForce = 100f;

    public Animator animator1;
    public Animator animator2;
    private float EnemyDist;

    void Awake()
    {
        muzzleflash = GetComponent<MuzzleFlash>();
        viewCamera = Camera.main;

        animator1.SetBool("Trigger", true);
        animator2.SetBool("Trigger", true);
    }

    void Start()
    {
        muzzleVelocity = 35;
        shootInterval = 100;
        force = 300f;
        EnemyDist = 100f;

        interval = 0.3f;
    }

    void Update()
    {
        if(Input.GetButton("AButton"))
        {
            Shoot();
         //  animator.SetBool("shoot", true);
        }

        rayTime += Time.deltaTime;
        if(rayTime >= interval)
        {
            CheckEnemy();
            rayTime = 0.0f;
        }
        

    }

    public void Shoot()
    {
        if(Time.time > nextShootTime)
        {
            nextShootTime = Time.time + shootInterval / 1000;

            //Vector3 crossHairScreenPos = UIMgr.camUI.WorldToScreenPoint(UIMgr.goCrossHairs[0].transform.position);
            //Ray ray = viewCamera.ScreenPointToRay(crossHairScreenPos);
            //Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);

            Ray ray = viewCamera.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0));

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green, 2f);
                if (hit.collider.gameObject.tag == "ENEMY")
                {
                    Instantiate(shootEffect, hit.point, Quaternion.identity);
                    EnemyHealth health = hit.collider.GetComponent<EnemyHealth>();
                    if (health != null)
                    {
                        health.Damage(gundamage);
                    }
                     //hit.rigidbody.AddForce(-hit.normal * hitForce);
                }
            }

            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleflash.Activate();
        }
    }

    public void CheckEnemy()
    {
        check_ray = viewCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(check_ray, out hit, Mathf.Infinity))
        {
            Debug.DrawRay(check_ray.origin, check_ray.direction * 100f, Color.green, 2f);
            if (hit.collider.gameObject.tag == "ENEMY")
            {
                float dist = Vector3.Distance(gameObject.transform.position, hit.collider.transform.position);
                if (dist <= EnemyDist)
                {
                    animator1.SetBool("Trigger", false);
                    animator2.SetBool("Trigger", false);
                }
            }
            else
            {
                animator1.SetBool("Trigger", true);
                animator2.SetBool("Trigger", true);
            }
        }

    }
}
