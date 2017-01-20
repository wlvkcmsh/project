using UnityEngine;
using System.Collections;

public class movetest : MonoBehaviour {
	public float MoveSpeed=10f;
	public float rotSpeed=20;
	// Use this for initialization
	void Start () {
		 float MoveSpeed=10f;
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate (MoveSpeed*Input.GetAxis ("Horizontal") * Time.deltaTime, 0f,MoveSpeed* Input.GetAxis ("Vertical") * Time.deltaTime);
	}


	void OnMouseDrag()
	{
		float rotX = Input.GetAxis ("Mouse X") * rotSpeed * Mathf.Deg2Rad;
		float rotY = Input.GetAxis ("Mouse X") * rotSpeed * Mathf.Deg2Rad;

		transform.RotateAround(Vector3.up,-rotX);
		transform.RotateAround(Vector3.right,rotY);
	}
}
