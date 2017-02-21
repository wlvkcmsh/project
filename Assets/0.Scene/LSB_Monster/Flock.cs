using UnityEngine;
using System.Collections;

public class Flock : MonoBehaviour {

    public float speed = 0.1f;

    public GameObject Monster_Center;

    float rotationSpeed = 4.0f;
    //Vector3 averageHeading;
    //Vector3 averagePosition;
    float neighbourDistance = 3.0f;

    bool turning = false;
	// Use this for initialization
	void Start ()
    {
        speed = Random.Range(0.5f, 1);

        Monster_Center = GameObject.FindGameObjectWithTag("Monster_Center");
	}
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Distance( this.transform.position, Monster_Center.transform.position)  >= globalFlock.MoveSize) // 원점과 현재 좌표 비교후 일정크기로 상태정해주기
        {
            Debug.Log(Monster_Center.transform.position);
            turning = true;
        }
        else
        {
            turning = false;
        }

        if(turning) // 오브젝트 나가는 방향에서 돌아올떄
        {
            Vector3 direction = Monster_Center.transform.position - transform.position; // 거리 구해주기
            transform.rotation = Quaternion.Slerp(transform.rotation, // 움직이는쪽으로 방향 로테이션
                Quaternion.LookRotation(direction),
                rotationSpeed * Time.deltaTime * 3); // 로테이션 스피드 
            speed = Random.Range(0.5f, 1);
        }
        else // 안쪽방향으로 오고 있을시
        {
            if (Random.Range(0, 5) < 1) // 룰적용
                ApplyRules();
        }
        
        transform.Translate(0, 0, Time.deltaTime * speed * 40); // 몬스터 들의 이동속도
	}

    void ApplyRules()
    {
        GameObject[] gos; // 몬스터 배열 가지고 오기
        gos = globalFlock.allMonster;

        Vector3 vcentre = Monster_Center.transform.position; // 군집의 중앙
        Vector3 vavoid = Monster_Center.transform.position;

        float gSpeed = 0.1f;

        Vector3 goalPos = globalFlock.goalPos; // 군집하는 원 점 

        float dist;

        int groupSize = 0;

        foreach(GameObject go in gos) // foreach 반복자 사용 하여 오브젝트 순회
        {
            if(go != this.gameObject) // 현재의 오브젝트와 동일한 오브잭트가 아니면
            {
                dist = Vector3.Distance(go.transform.position, this.transform.position); // 다른 오브잭트와 거리 구하기
                if(dist <= neighbourDistance) // 지정거리 가 더크면
                {
                    vcentre = go.transform.position;
                    groupSize++;

                    if (dist < 2.0f)
                    {
                        vavoid = vavoid + (this.transform.position - go.transform.position);
                    }

                    Flock anotherFlock = go.GetComponent<Flock>();
                    gSpeed = gSpeed + anotherFlock.speed;
                }
            }
        }

        if(groupSize > 0)
        {
            Debug.Log("groupSize == 0");
            vcentre = vcentre / groupSize + (goalPos - this.transform.position);
            speed = gSpeed / groupSize;

            Vector3 direction = (vcentre + vavoid) - transform.position;
            if (direction != Monster_Center.transform.position)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
        }

    }
}
