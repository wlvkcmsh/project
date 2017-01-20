using UnityEngine;
using System.Collections;

public class AnTransparent : MonoBehaviour {
    private Renderer r;
	// Use this for initialization
	void Start () {
        r = gameObject.GetComponent<Renderer>();
        r.material.color = new Color(1,1,1,0.5f);

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
