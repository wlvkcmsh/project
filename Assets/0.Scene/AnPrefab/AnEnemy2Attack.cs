using UnityEngine;
using System.Collections;

public class AnEnemy2Attack : MonoBehaviour {
    public AnEnemyFire enemy1;
    public AnEnemyFire enemy2;
    public GameObject enemy3;
    public GameObject missile;
  
    // Use this for initialization
    void Start () {
       

    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public void Call()
    {
        enemy1.attack = true;
        enemy2.attack = true;
    }
    public void MissileFire()
    {
        enemy1.attack = false;
        enemy2.attack = false;
        missile.SetActive(true);
        enemy3.SetActive(true);
    }
}
