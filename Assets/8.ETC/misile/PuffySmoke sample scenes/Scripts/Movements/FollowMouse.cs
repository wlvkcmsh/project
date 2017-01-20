using UnityEngine;
using System.Collections;

public class FollowMouse : MonoBehaviour {
	
	public float depth = 20f;
	
	Transform _transform;
	
	// Use this for initialization
	void Start () {
		_transform = transform;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButton(1) ) _transform.position = Camera.main.ScreenToWorldPoint(new Vector3 (Input.mousePosition.x, Input.mousePosition.y, depth) );

	}
}
