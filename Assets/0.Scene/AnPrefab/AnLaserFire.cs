using UnityEngine;
using System.Collections;

public class AnLaserFire : MonoBehaviour {
    public enum AttackState { idle, attack, die };
    public AttackState towerState = AttackState.idle;

    private Transform[] target = new Transform[4];

    private int rotationSpeed = 10;
    private bool isDie = false;
    //총알에 관한 변수 
    public GameObject bullet;
    public Transform firePos;
    public Transform firePos2;
    private float AttackBulletTime = 0.0f;
    private int count = 0;
    private int time = 1;
    private float StartAttackTime = 0.0f;
    // Use this for initialization
    void Start () {
        for(int i=0; i <4; i++) {
            target[i] = GameObject.Find("BigEnemy/target" + (i+1)).GetComponent<Transform>(); //tranform 할당
        }
        StartCoroutine(this.AttackAction());
    }
    IEnumerator AttackAction()
    {
        while (!isDie)
        {

            switch (towerState)
            {
                case AttackState.idle:
                    StartAttackTime += Time.deltaTime;
                    if(StartAttackTime > 13.0f)
                    {
                        StartAttackTime = 0.0f;
                        towerState = AttackState.attack;
                    }
                    break;

                case AttackState.attack:
                    AttackBulletTime += Time.deltaTime;
                    transform.rotation = (Quaternion.Slerp(transform.rotation,
                                                         Quaternion.LookRotation(target[count].position - transform.position),
                                                           rotationSpeed * Time.deltaTime));
                    if (AttackBulletTime > time)
                    {
                        time = Random.Range(1, 5);
                        AttackBulletTime = 0.0f;
                        Fire();
                        count = Random.Range(0, 4);
                        Debug.Log(count);

                    }
                    //stateM = 1;

                    


                    break;


            }
            yield return null;
        }

    }
    void Fire()
    {
        

        Instantiate(bullet, firePos.position, firePos.rotation);
        Instantiate(bullet, firePos2.position, firePos2.rotation);
       

    }
    // Update is called once per frame
    void Update () {
	
	}
}
