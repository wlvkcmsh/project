using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ObjectManager : MonoBehaviour
{
    public GameObject[] enemyPref;
    public List<GameObject> enemyListPool = new List<GameObject>();

    //public GameObject[] turretPref;
    //public List<GameObject> turretListPool = new List<GameObject>();

    //public GameObject[] fuelPref;
    //public List<GameObject> fuelListPool = new List<GameObject>();


    public Transform[] objectSpawn ;
    public List<GameObject> objectListPool = new List<GameObject>();

    public float createTime = 1f;
    public int maxObject = 12;

    public bool isGameOver = false;

    public int objectCount;


    public int curEvent=0;
    bool isEvent;

    private static ObjectManager s_Instance = null;

    public static ObjectManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance= FindObjectOfType(typeof(ObjectManager)) as ObjectManager;
            }
            return s_Instance;
        }
    }

    void Awake()
    {
        if (s_Instance != null)
        {
            Debug.LogError("Cannot have two instances of ObjectManager.");
            return;
        }
        s_Instance = this;
        DontDestroyOnLoad(this);
        Debug.Log("ObjectManager Awake");
    }

    void Start()
    {

        init();
        StartCoroutine(this.createObject());
    }
    public void init()
    {
        objectCount = 0;

        isEvent = true;

        //오브젝트 풀링. 미리 생성해놓음
        for (int i = 0; i < 10; i++)
        {
            GameObject _enemy = (GameObject)Instantiate(enemyPref[UnityEngine.Random.Range(0, enemyPref.Length)]);
            _enemy.name = "Enemy_" + i.ToString();
            _enemy.transform.parent = this.gameObject.transform;
            _enemy.transform.localScale = new Vector3(1f, 1f, 1f);
            _enemy.SetActive(false);
            enemyListPool.Add(_enemy);
        }

        ////오브젝트 풀링. 미리 생성해놓음
        //for (int i = 0; i < 10; i++)
        //{
        //    GameObject _turret = (GameObject)Instantiate(turretPref[UnityEngine.Random.Range(0, turretPref.Length)]);
        //    _turret.name = "Turret_" + i.ToString();
        //    _turret.transform.parent = this.gameObject.transform;
        //    _turret.transform.localScale = new Vector3(1f, 1f, 1f);
        //    _turret.SetActive(false);
        //    turretListPool.Add(_turret);
        //}
        
        ////오브젝트 풀링. 미리 생성해놓음
        //for (int i = 0; i < 10; i++)
        //{
        //    GameObject _fuel = (GameObject)Instantiate(fuelPref[UnityEngine.Random.Range(0, fuelPref.Length)]);
        //    _fuel.name = "Fuel_" + i.ToString();
        //    _fuel.transform.parent = this.gameObject.transform;
        //    _fuel.transform.localScale = new Vector3(1f, 1f, 1f);
        //    _fuel.SetActive(false);
        //    fuelListPool.Add(_fuel);
        //}

       
    }



    IEnumerator createObject()
    {
        while (!isGameOver)
        {
            if (isGameOver) yield break;

            objectSpawn = transform.GetChild(0).GetChild(curEvent).GetComponentsInChildren<Transform>();
            
            yield return new WaitForSeconds(createTime);

            if (isEvent)
            {
                for (int i = 1; i < objectSpawn.Length; i++)
                {
                    if (objectSpawn[i].gameObject.tag == "ENEMY")
                    {
                        foreach (GameObject _enemy in enemyListPool)
                        {
                            if (!_enemy.activeSelf) //비활성화 여부로 사용 가능한 몬스터를 판단.
                            {
                                _enemy.transform.position = objectSpawn[i].position;
                                _enemy.SetActive(true);
                                objectCount++;
                                break;
                            }
                        }
                    }
                    //else if (objectSpawn[i].gameObject.tag == "TURRET")
                    //{
                    //    foreach (GameObject _turret in turretListPool)
                    //    {
                    //        if (!_turret.activeSelf) //비활성화 여부로 사용 가능한 몬스터를 판단.
                    //        {
                    //            _turret.transform.position = objectSpawn[i].position;
                    //            _turret.SetActive(true);
                    //            objectCount++;

                    //            break;
                    //        }
                    //    }
                    //}
                    //else if (objectSpawn[i].gameObject.tag == "FUEL")
                    //{
                    //    foreach (GameObject _fuel in fuelListPool)
                    //    {
                    //        if (!_fuel.activeSelf) //비활성화 여부로 사용 가능한 몬스터를 판단.
                    //        {

                    //            _fuel.transform.position = objectSpawn[i].position;
                    //            _fuel.SetActive(true);
                    //            objectCount++;

                    //            break;
                    //        }
                    //    }
                    //}

                    isEvent = false;
                }
            }
        }
    }
    
 
  
    public void ClearObject()
    {
        foreach (GameObject monster in objectListPool)
        {
            if (monster.activeSelf) //비활성화 여부로 사용 가능한 몬스터를 판단.
            {
                monster.SetActive(false);
                objectCount--;
            }
        }
    }
}
