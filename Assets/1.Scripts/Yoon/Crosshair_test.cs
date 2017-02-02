using UnityEngine;
using System.Collections;

public class Crosshair_test : MonoBehaviour {

    public float distance_z;

    // Use this for initialization
    void Start()
    {
        distance_z = 5;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = distance_z;
        transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
    }
}
