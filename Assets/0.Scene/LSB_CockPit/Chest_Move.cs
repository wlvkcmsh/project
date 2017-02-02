using UnityEngine;
using System.Collections;

public class Chest_Move : MonoBehaviour {

    public GameObject Cam;
    public float time = 0.5f;
    void Start () {

	}
    void Update()
    {

    }
	void FixedUpdate () {
       
        transform.rotation = Quaternion.Lerp(transform.rotation, Cam.transform.rotation, time);
    }
}
