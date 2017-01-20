using UnityEngine;
using System.Collections;

public class AnEnemy2AniCtl : MonoBehaviour {
    public bool aniStart = false;
    Animator ani;
	// Use this for initialization
	void Start () {
        ani = gameObject.GetComponent<Animator>();
	}

    // Update is called once per frame
    void Update() {
        if (aniStart == true)
        {
            ani.SetBool("ani",true);
        }
	}
}
