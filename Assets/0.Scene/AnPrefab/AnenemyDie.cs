using UnityEngine;
using System.Collections;

public class AnenemyDie : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "ENEMY")
        {
            other.gameObject.GetComponentInChildren<BoxCollider>().enabled = false;
            Destroy(other.gameObject, 1.0f);
        }

    }
}
