using UnityEngine;
using System.Collections;

public class AnimRandomStart : MonoBehaviour {

    Animator anim;
	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();

        float startPoint = Random.Range(0f, 1f);
        anim.Play("assault_mech_run", -1, startPoint);


    }

  
}
