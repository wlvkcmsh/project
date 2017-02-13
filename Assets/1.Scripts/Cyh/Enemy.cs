using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{

    public Transform muzzle;
    public Projectile_test projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;

    float nextShotTime;

    public GameObject shootEffect;
    public int currentHealth ;


    //추적 공격
    private float attackDist = 50f;
    private bool isDie = false;
    private Transform monsterTr;
    private Transform playerTr;
    private NavMeshAgent nvAgent;

    public Transform waypoint;
   // int currentwaypoint = 0;
    float rotationControl = 6.0f;
    public enum MonsterState
    {
        idle, trace, attack, die
    };

    public MonsterState monsterState = MonsterState.idle;
   
    void Start()
    {
        monsterTr = this.gameObject.GetComponent<Transform>();
        playerTr = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
       // nvAgent = this.gameObject.GetComponent<NavMeshAgent>();
        StartCoroutine(this.CheckMonsterState());
        StartCoroutine(this.MonsterAction());
        waypoint = GameObject.Find("Robot").transform;
    }

    IEnumerator CheckMonsterState()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.2f);

            float dist = Vector3.Distance(playerTr.position, monsterTr.position);

            if (dist <= attackDist)
            {
                monsterState = MonsterState.attack;
            }
            
        }
    }

    IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            switch (monsterState)
            {
                case MonsterState.idle:
                   // nvAgent.Stop();
                    break;
                case MonsterState.trace:
                   // nvAgent.destination = playerTr.position;
                    break;
                case MonsterState.attack:
                    Shoot();
                    break;
            }
            yield return null;
        }
    }
   



    public void Damage(int damageAmount)
    {
        
        currentHealth -= damageAmount;

       
        if (currentHealth <= 0)
        {
            Instantiate(shootEffect, gameObject.transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }


    public void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            nextShotTime = Time.time + msBetweenShots / 1000;
            Projectile_test newProjectole = Instantiate(projectile, muzzle.position, muzzle.rotation) as Projectile_test;
            newProjectole.SetSpeed(muzzleVelocity);
        }
    }

    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            Shoot();
        }
        Turn();
    }

    void Turn()
    {
        Quaternion ratation = Quaternion.LookRotation(waypoint.position - transform.position);
        //다음 waypoint로 회전
        transform.rotation = Quaternion.Slerp(transform.rotation, ratation, Time.deltaTime * rotationControl);
    }





}