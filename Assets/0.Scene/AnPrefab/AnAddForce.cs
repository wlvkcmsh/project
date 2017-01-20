using UnityEngine;
using System.Collections;

public class AnAddForce : MonoBehaviour {
    private Rigidbody rb;
    public float thrust;
	// Use this for initialization
	void Start () {
        rb = gameObject.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        rb.AddForce(transform.forward * thrust);
	}
}
