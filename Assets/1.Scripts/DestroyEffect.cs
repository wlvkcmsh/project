using UnityEngine;
using System.Collections;

public class DestroyEffect : MonoBehaviour {
	private float delTime = 0.0f;
	public AudioClip bombSfx;
	private AudioSource source = null;
	void start()
	{
		source = GetComponent<AudioSource>();
		source.PlayOneShot (bombSfx,0.9f);
	}
	void Update ()
	{
	
		delTime += Time.deltaTime;
		if (delTime >= 2.0f) {
			Destroy (transform.gameObject);
		}
	
	}
}
