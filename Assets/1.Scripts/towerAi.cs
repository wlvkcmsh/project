using UnityEngine;
using System.Collections;

public class towerAi : MonoBehaviour {

	public enum TowerState{	idle, attack, die	};
	
	public TowerState towerState = TowerState.idle;
	//추적 사정거리
	public float traceDist = 30.0f;
	//공격 사정거리 
	public GameObject expEffect;
	private bool isDie = false;
	private Transform towerTr;
	private Transform playerTr;
	private int Sparkstate = 1;
	private Animator animator;
	//private float SpeedTime = 0.0f;
	//private float SpeedAniTime = 0.0f;
	//private int stateM = 0;
	private int towerHp = 50;
	public GameObject Tower;
	private GameObject spark = null;
	private int rotationSpeed = 10;
	public GameObject sparkEffect;

	//총알에 관한 변수 
	public GameObject bullet;
	public Transform firePos;
	public Transform firePos2;
	private float TowerBulletTime = 0.0f;
	public AudioClip fireSfx;

	private AudioSource source = null;



	void Start () {

		towerTr = this.gameObject.GetComponent<Transform> ();
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>(); //player tranform 할당
		//NavMeshAgent 컴포넌트 할당
		//일정한 간격으로 몬스터의 행동 상태를 체크하는 코루틴 함수 실행
		StartCoroutine (this.CheckTowerState ());
		//몬스터의 상태에 따라 동작하는 루틴을 실행하는 코루틴 함수 실행
		StartCoroutine (this.TowerAction());
		source = GetComponent<AudioSource>();

	}
	IEnumerator CheckTowerState()
	{
		while (!isDie) {
			//0.2초 동안 기다렸다가 다음으로 넘어감
			yield return new WaitForSeconds(0.2f);
			//몬스터와 플레이어 사이의 거리 측정
			float dist = Vector3.Distance(playerTr.position, towerTr.position);
			
			//공격거리 범위 이내로 들어왔는지 확인
			if(dist <= traceDist)
			{
				towerState = TowerState.attack;
				
			}
			//추적거리 범위 이내로 들어왔는지 확인

			else
			{
				towerState = TowerState.idle;
			}
		}
	}
	IEnumerator TowerAction()
	{
		while (!isDie) {
			switch (towerState) {
			case TowerState.idle:

				break;
		
			case TowerState.attack:
				TowerBulletTime += Time.deltaTime;
				if(TowerBulletTime > 0.5f){
					TowerBulletTime = 0.0f;
					Fire();
				}
				//stateM = 1;

				transform.rotation = (Quaternion.Slerp(transform.rotation, 
				                                      Quaternion.LookRotation(playerTr.position -   transform.position), 
				                                        rotationSpeed * Time.deltaTime));


			
				break;
				
				
			}
			yield return null;
		}
		
	}
	void Fire(){
		CreateBullet ();
		
		source.PlayOneShot (fireSfx,0.9f);
	}
	void CreateBullet(){
		Instantiate (bullet, firePos.position, firePos.rotation);
		Instantiate (bullet, firePos2.position, firePos2.rotation);
	}

	void OnCollisionEnter(Collision coll)
	{

		
	}
    public void HitTower(int damage)
    {
        if (Sparkstate == 1 && towerHp <= 30)
        {
            Sparkstate = 2;
            spark = (GameObject)Instantiate(sparkEffect, gameObject.transform.position, Quaternion.identity);
        }
        traceDist = 50;

        
        towerHp -= damage;
        if (towerHp <= 0)
        {

            Instantiate(expEffect, towerTr.position, Quaternion.identity);
            Destroy(spark);
            Destroy(Tower);
            TowerDie();
        }
    }
	void TowerDie()
	{

		StopAllCoroutines ();
		isDie = true;
		towerState = TowerState.die;
	
		//animator.SetTrigger("IsDie");
		
		Tower.GetComponentInChildren<CapsuleCollider> ().enabled = false;
		foreach (Collider coll in Tower.GetComponentsInChildren<SphereCollider>()) {
			coll.enabled = false;
		}
	}
	// Update is called once per frame
	void Update () {

		//// 방향을 구하고,
		//Vector3 _dir = (playerTr.position - transform.position).normalized;
		//// 방향을 바라보는 Quaternion을 구한다.
		//Quaternion _rot = Quaternion.LookRotation( _dir );
		
		//float ang = Quaternion.Angle(transform.rotation, playerTr.rotation);
		
		
		
	}

}
