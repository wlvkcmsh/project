using UnityEngine;
using System.Collections;

public class setSpeedBullet : MonoBehaviour
{

    public float speed = 0.5f;
    public Transform firepos;
    public Transform despos;

    private Vector3 pos;

    // Use this for initialization
    void Start()
    {
        //pos = firepos.position - despos.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0,0,-firepos.position.z*Time.deltaTime);
    }
}
