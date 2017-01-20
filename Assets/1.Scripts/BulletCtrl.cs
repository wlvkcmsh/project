using UnityEngine;
using System.Collections;

public class BulletCtrl : MonoBehaviour {
    public int damage = 20; //파괴력
    public float speed = 1000.0f; //발사 속도
    private float bulleTime = 0.0f;
    public GameObject bombparticle;
   


    // Use this for initialization
    void Start () {
        GetComponent<Rigidbody>().AddForce(transform.forward * speed);
    }
	
	// Update is called once per frame
	void Update () {
        bulleTime += Time.deltaTime;
        if(bulleTime > 2.0f)
        {
            
            Destroy(gameObject);
        }

    }
    void OnTriggerEnter(Collider collision)
    {
        
        if (collision.gameObject.tag == "Player")
        {
            //Invoke("destroy", 4.0f);
            Instantiate(bombparticle, gameObject.transform.position, Quaternion.identity);
            GameManager.Instance.LifeMinus(1);
            //bombparticle.SetActive(true);
            Destroy(gameObject);
        }
            

    }
    //void destroy()
    //{
    //    Destroy(gameObject);
    //}

}
