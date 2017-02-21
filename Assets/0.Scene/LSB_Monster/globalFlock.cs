using UnityEngine;
using System.Collections;

public class globalFlock : MonoBehaviour {

    public GameObject MonsterPrefab;
    public GameObject goalPrefab;

    public GameObject Monster_Center; // 몬스터 군집 중앙

    public static int MoveSize = 10;

    static int numMonster = 20;

    public static GameObject[] allMonster = new GameObject[numMonster]; // 몬스터 배열

    public static Vector3 goalPos = Vector3.zero; // 군집하는 점
    
	// Use this for initialization
	void Start ()
    {
	    for(int i = 0; i < numMonster; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-MoveSize + Monster_Center.transform.position.x, MoveSize + Monster_Center.transform.position.x),
                                      Random.Range(-MoveSize + Monster_Center.transform.position.y, MoveSize + Monster_Center.transform.position.y),
                                      Random.Range(-MoveSize + Monster_Center.transform.position.z, MoveSize + Monster_Center.transform.position.z));
            allMonster[i] = (GameObject)Instantiate(MonsterPrefab, pos, Quaternion.identity);
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        //if(Random.Range(0,10000) < 50) // 랜덤으로 위치 지정
        //   {
        //       goalPos = new Vector3(Random.Range(-MoveSize, MoveSize),
        //                             Random.Range(-MoveSize, MoveSize),
        //                             Random.Range(-MoveSize, MoveSize));
        //       goalPrefab.transform.position = goalPos;
        //   }

        goalPos = Monster_Center.transform.position;

    }
}
