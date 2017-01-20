using UnityEngine;
using System.Collections;

public class AnEnemyFire : MonoBehaviour {
    public enum EnemyType { one, two, tree };
    public EnemyType enemyType;
    public GameObject firepos;  
    private Transform playerTr;
    public float rotationSpeed;
    public bool attack = false;
    private float bulletCulTime = 0.0f;
    public GameObject bulletbomb;
    public GameObject eff;
    public AnEnemy2AniCtl ani02;
    // Use this for initialization
    void Start () {
        playerTr = GameObject.FindWithTag("Player").transform;
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 relativePos = playerTr.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos);
        transform.rotation = rotation;
        
        if(attack == true && ani02.aniStart == false)
        {
            bulletCulTime += Time.deltaTime;
            if (enemyType == EnemyType.one && bulletCulTime > 6.0f)
            {
                bulletCulTime = 0.0f;
                CreateBullet();
                gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.LookRotation(
                playerTr.position - transform.position), rotationSpeed * Time.deltaTime);
            }
            else if (enemyType == EnemyType.two && bulletCulTime > 8.0f)
            {
                bulletCulTime = 0.0f;
                CreateBullet();
                gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.LookRotation(
                playerTr.position - transform.position), rotationSpeed * Time.deltaTime);
            }
        }
          
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "MISSILE")
        {
            ani02.aniStart = true;
            Destroy(gameObject);
            Destroy(other.gameObject);
            Instantiate(eff, firepos.transform.position, firepos.transform.rotation);

        }

    }
    void CreateBullet()
    {
        Instantiate(bulletbomb, firepos.transform.position, firepos.transform.rotation);
        
    }

}
