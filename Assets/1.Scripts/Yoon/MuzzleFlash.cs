using UnityEngine;
using System.Collections;

public class MuzzleFlash : MonoBehaviour {

    public GameObject flashPosition;

    public float flashtime;

	void Start () {
        Deactivate();
	}
	
	public void Activate()
    {
        flashPosition.SetActive(true);

        Invoke("Deactivate", flashtime);
    }

    void Deactivate()
    {
        flashPosition.SetActive(false);
    }
}
