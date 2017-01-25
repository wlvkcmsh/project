using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float turnSpeed;

    public float rotateAngle;

    public float MaxRotZ;
    public float MinRotZ;
    public float MaxRotY;
    public float MinRotY;

    public float rotZ;
    public float rotY;

    // Use this for initialization
    void Start () {
        turnSpeed = 2f;
        rotateAngle = Mathf.PI / 3;

        MaxRotZ = 50f;
        MinRotZ = -50f;
        MaxRotY = 40f;
        MinRotY = -20f;
	}
	
	// Update is called once per frame
	void Update () {

        /*
        rotZ = transform.localEulerAngles.x + (turnSpeed * Time.deltaTime);
        rotY = transform.localEulerAngles.y + (turnSpeed * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(new Vector3(0f, rotY, rotZ));
        */

        
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.up, -turnSpeed + Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.up, turnSpeed + Time.deltaTime);
        }

        //transform.position.y = Mathf.Clamp(transform.position.y, MinRotY, MaxRotY);

        rotZ = transform.localEulerAngles.x;
        rotY = transform.localEulerAngles.y;
        
    }

    void CheckLimit()
    {
        if (rotZ > MaxRotZ) rotZ = MaxRotZ;
        if (rotZ < MinRotZ) rotZ = MinRotZ;
        if (rotY > MaxRotY) rotY = MaxRotY;
        if (rotY < MinRotY) rotY = MinRotY;
    }
}
