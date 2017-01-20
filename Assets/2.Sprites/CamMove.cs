using UnityEngine;
using System.Collections;

public class CamMove : MonoBehaviour {
    public Camera CamLeft;
    public Transform lookTarget;
	// Use this for initialization
    // Update is called once per frame

	void Update ()
    {
        if (Input.GetKey("-"))
        {
            transform.Translate(Vector3.right * 0.001f);
            CamLeft.transform.Translate(-Vector3.right * 0.001f);
        }
        
        if (Input.GetKey("="))
        {
            transform.Translate(-Vector3.right * 0.001f);
            CamLeft.transform.Translate(Vector3.right * 0.001f);
        }

        //if(Input.GetKey("0"))
        //{
        //    lookTarget.Translate(Vector3.forward * 0.01f);

        //}
        //else if(Input.GetKey("9"))
        //{
        //    lookTarget.Translate(-Vector3.forward * 0.01f);

        //}
        ////Debug.Log(Vector3.Distance(transform.position, CamLeft.transform.position));
        transform.LookAt(lookTarget);
        CamLeft.transform.LookAt(lookTarget);
    }
}
