using UnityEngine;
using System.Collections;

public class GunAni : MonoBehaviour {

    public enum GunState { idle, walk, shoot };
    public GunState gunState = GunState.idle;

    private Animator animator;

    public bool gameover;

    void Start()
    {
        gameover = false;

        animator = gameObject.GetComponent<Animator>();

        StartCoroutine(this.GunAction());
    }

    IEnumerator GunAction()
    {

        yield return null;
    }

}
