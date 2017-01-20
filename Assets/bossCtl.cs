using UnityEngine;
using System.Collections;

public class bossCtl : MonoBehaviour {


    private bool isDie = false; //몬스터의 사망 여부
    
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

//public class enemy : MonoBehaviour
//{
//    public enum MonsterState { idle, trace, attack, die }; //몬스터 상태
//    public enum MonsterType { one, two, tree };
//    public MonsterState monsterState = MonsterState.idle; //몬스터의 현재 상태 정보를 저장
//    public MonsterType monsterType;
//    private Transform enemyTr;
//    private Transform playerTr;

//    // public float traceDist = 10.0f; //추적 사정거리
//    public float attackDist = 2.0f; //공격 사정거리

//    private bool isDie = false; //몬스터의 사망 여부

//    public GameObject bloodEffect; //혈흔효과
//    public GameObject bloodDecal; //데칼효과
//    private Animator animator;
//    private int hp = 100; //몬스터 체력
//    private float runtime = 0.0f;
//    private Rigidbody rb;
//    public float rotationSpeed;

//    public GameObject bombpaticle; //터지는 파티클
//    public GameObject upbody;
//    //public GameObject downbody;
//    // Use this for initialization
//    void Start()
//    {
//        rb = this.gameObject.GetComponent<Rigidbody>();
//        enemyTr = this.gameObject.GetComponent<Transform>(); //몬스터 transform 할당
//        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>(); //player tranform 할당
//        //nvAgent = this.gameObject.GetComponent<NavMeshAgent>(); //NavMeshAgent 할당
//        animator = this.gameObject.GetComponent<Animator>(); //애니메이터 할당

//        //nvAgent.destination = playerTr.position; //추적 대상의 위치를 설정하면 바로 추적 시작

//        // StartCoroutine(this.CheckMonsterState()); //몬스터의 행동상태를 체크
//        StartCoroutine(this.MonsterAction()); //상태에 따라 동작 


//    }

//    void OnEnable()
//    {
//        PlayerCtrl.OnPlayerDie += this.OnPlayerDie;
//    }

//    void OnDisable()
//    {
//        PlayerCtrl.OnPlayerDie -= this.OnPlayerDie;
//    }

//    //IEnumerator CheckMonsterState()
//    //{
//    //    while (!isDie)
//    //    {
//    //        yield return new WaitForSeconds(0.2f); //0.2초 기다렸다가 다음으로 넘어감
//    //        //float dist = Vector3.Distance(playerTr.position, monsterTr.position); //몬스터와의 사정거리 측정

//    //        //if (dist <= attackDist)
//    //        //{
//    //        //    monsterState = MonsterState.attack;
//    //        //}
//    //        //else if (dist <= traceDist)
//    //        //{
//    //        //    monsterState = MonsterState.trace;
//    //        //}
//    //        //else
//    //        //{
//    //        //    monsterState = MonsterState.idle;
//    //        //}
//    //    }
//    //}

//    IEnumerator MonsterAction()
//    {
//        while (!isDie)
//        {
//            switch (monsterState)
//            {
//                case MonsterState.idle:
//                    // nvAgent.Stop();

//                    animator.SetBool("isTrace", false);

//                    monsterState = MonsterState.trace;  //type - one 바로 앞으로 움직이고 쏘고
//                    break;

//                case MonsterState.trace:
//                    // nvAgent.destination = playerTr.position;
//                    //  nvAgent.Resume();

//                    animator.SetBool("isAttack", false);
//                    animator.SetBool("isIdle", false);
//                    animator.SetBool("isTrace", true);
//                    break;

//                case MonsterState.attack:

//                    Vector3 relativePos = playerTr.position - upbody.transform.position;
//                    Quaternion rotation = Quaternion.LookRotation(relativePos);
//                    upbody.transform.rotation = rotation;
//                    gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.LookRotation(playerTr.position - upbody.transform.position), rotationSpeed * Time.deltaTime);
//                    //  nvAgent.Stop();
//                    //animator.speed = 0.3f;
//                    animator.SetBool("isIdle", false);
//                    animator.SetBool("isTrace", false);
//                    animator.SetBool("isAttack", true);
//                    break;
//            }
//            yield return null;
//        }
//    }
//    public void Hit(int damage)
//    {
//        hp -= damage;
//        if (hp <= 0)
//        {
//            MonsterDie();
//        }
//    }
//    void OnCollisionEnter(Collision coll)
//    {
//        if (coll.gameObject.tag == "BULLET")
//        {
//            CreateBloodEffect(coll.transform.position); //혈흔효과 함수 호출

//            hp -= coll.gameObject.GetComponent<BulletCtrl>().damage;
//            if (hp <= 0)
//            {
//                MonsterDie();
//            }

//            Destroy(coll.gameObject); //총알 삭제
//                                      //  animator.SetTrigger("isHit");
//        }
//    }

//    void MonsterDie()
//    {
//        StopAllCoroutines();
//        bombpaticle.SetActive(true);
//        isDie = true;
//        monsterState = MonsterState.die;
//        //  nvAgent.Stop();
//        animator.SetTrigger("isDie");

//        gameObject.GetComponentInChildren<BoxCollider>().enabled = false;

//        foreach (Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
//        {
//            coll.enabled = false;
//        }
//        Destroy(gameObject, 1.0f); //1초 뒤 삭제
//    }

//    void CreateBloodEffect(Vector3 pos)
//    {
//        GameObject blood1 = (GameObject)Instantiate(bloodEffect, pos, Quaternion.identity); //혈흔효과 생성
//        Destroy(blood1, 2.0f); //2초 뒤 삭제

//        Vector3 decalPos = enemyTr.position + (Vector3.up * 0.05f);
//        Quaternion decalRot = Quaternion.Euler(90, 0, Random.Range(0, 360));

//        GameObject blood2 = (GameObject)Instantiate(bloodDecal, decalPos, decalRot);
//        float scale = Random.Range(1.5f, 3.5f);
//        blood2.transform.localScale = Vector3.one * scale;

//        Destroy(blood2, 5.0f);
//    }

//    void OnPlayerDie()
//    {
//        StopAllCoroutines();
//        //  nvAgent.Stop();
//        //   animator.SetTrigger("isPlayerDie");
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        runtime += Time.deltaTime;

//        if (runtime >= 3.0f && monsterState != MonsterState.attack && monsterState != MonsterState.die)
//        {
//            //enemyTr.rotation = Quaternion.Slerp(enemyTr.rotation, Quaternion.LookRotation(playerTr.position - transform.position), rotationSpeed * Time.deltaTime);
//            monsterState = MonsterState.attack;

//            if (MonsterType.two == monsterType)  //two 타입이 점프를 하는 로봇
//                rb.AddForce(transform.up * 50000);
//        }
//        if (MonsterType.two == monsterType)
//        {
//            if (monsterState == MonsterState.trace)
//            {
//                enemyTr.position += enemyTr.forward * 10 * Time.deltaTime;
//            }
//        }

//    }
//}
