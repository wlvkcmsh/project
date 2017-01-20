using UnityEngine;
using System.Collections;

public class ShotBehavior : MonoBehaviour {
	private float DelBulletTime = 0.0f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		DelBulletTime += Time.deltaTime;
		if (DelBulletTime > 3.0f) {
			Destroy(gameObject.gameObject);
		}
		transform.position += transform.forward * Time.deltaTime * 40f;

	}
}
