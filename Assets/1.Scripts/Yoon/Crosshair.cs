using UnityEngine;
using System.Collections;

public class Crosshair : MonoBehaviour {

    Vector3 mousePos;
    public float distance;
    public Transform pos;

    private RaycastHit hit;
    private float range = 500;


    public GameObject bullet;

    void Start()
    {
        distance = 2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = Input.mousePosition;
        mousePos.z = distance;
        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
        GetWorldPositionOnPlane(transform.position, distance);
        if (Input.GetMouseButtonDown(0))
        {
            CollisionCheck();
            Debug.Log("Click!!!");
        }
    }

    void CollisionCheck()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.Log("Hit!!!");

            Debug.DrawRay(ray.origin, ray.direction*100f, Color.green, 10f);
        }
    }

    public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float z)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
        float distance;
        xy.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }
}
