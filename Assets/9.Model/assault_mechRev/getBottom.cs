using UnityEngine;
using System.Collections;

public class getBottom : MonoBehaviour {

    public Transform bottom;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.position = bottom.position;
	}
}
