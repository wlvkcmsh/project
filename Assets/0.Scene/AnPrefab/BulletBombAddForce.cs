using UnityEngine;
using System.Collections;

public class BulletBombAddForce : MonoBehaviour {
    private Rigidbody rb;
    private Transform playerTr;
    private Quaternion roate;
    public GameObject eff;
    // Use this for initialization
    void Start () {
        rb = this.gameObject.GetComponent<Rigidbody>();
        playerTr = GameObject.FindWithTag("AnAniPivot").transform;

        Vector3 relativePos = playerTr.position - transform.position;
        rb.AddForce(relativePos.normalized * 500);
       
    }
	
	// Update is called once per frame
	void Update () {
       


    }
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "AnAniPivot")
        {
            Instantiate(eff, transform.position, transform.rotation);
            Destroy(gameObject);
        }

    }
}
