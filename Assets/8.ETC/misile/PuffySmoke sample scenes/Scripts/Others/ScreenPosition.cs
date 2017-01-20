using UnityEngine;
using System.Collections;

public class ScreenPosition : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Camera cam = Camera.main.GetComponent<Camera>();
		
		transform.position = cam.ScreenToWorldPoint(new Vector3(Screen.width*0.5f,30,1));
		
	}
	
}
