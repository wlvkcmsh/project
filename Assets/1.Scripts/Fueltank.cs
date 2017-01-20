using UnityEngine;
using System.Collections;

public class Fueltank : MonoBehaviour
{
    private int hp = 30; //몬스터 체력
    public GameObject bombpaticle; //터지는 파티클
    private Rigidbody rb;
    //Use this for initialization
    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void HitFuel(int damage)
    {
        hp -= damage;
    
        if (hp <= 0)
        {
            fueltankbomb();
        }
    }
    void fueltankbomb()
    {
        //rb.AddForce(transform.up * 1000);
        bombpaticle.SetActive(true);
        gameObject.GetComponentInChildren<BoxCollider>().enabled = false;
        Destroy(gameObject, 1.0f); //1초 뒤 삭제
    }
}
