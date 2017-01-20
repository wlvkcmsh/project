using UnityEngine;
using System.Collections;

public class Fire : MonoBehaviour
{

    public GameObject bullet;
    public Transform firepos;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            fire();
        }
    }

    void fire()
    {
        Instantiate(bullet, firepos.position , firepos.rotation);
    }
}
