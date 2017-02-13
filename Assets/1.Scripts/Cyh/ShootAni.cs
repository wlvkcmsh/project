using UnityEngine;
using System.Collections;

public class ShootAni : MonoBehaviour {
    Animator animator;
    Gun gun;
    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
        gun = GetComponent<Gun>();
    }

    void Update()
    {
        if (Input.GetButton("AButton"))
        {
            gun.Shoot();
            animator.SetBool("shoot", true);
        }
    }
}
