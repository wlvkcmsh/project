using UnityEngine;
using System.Collections;

public class Leg_Move : MonoBehaviour {


    float speed = 20.0f;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        h = h * speed * Time.deltaTime;
        v = v * speed * Time.deltaTime;

        transform.Rotate(Vector3.up * h * 10);
        transform.Translate(Vector3.forward * v);
    }
       
}
