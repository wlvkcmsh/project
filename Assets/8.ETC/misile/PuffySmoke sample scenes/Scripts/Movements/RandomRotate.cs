using UnityEngine;
using System.Collections;

public class RandomRotate : MonoBehaviour {
	
	public float maxRandomSpeed = 10f;
	public float randomDelay = 5f;
	
	private double elapsedTime;
	private float delay;
	float ax;
	float ay;
	float az;
	// Use this for initialization
	void Start () {
		Init();
	}
	
	void Init(){
		ax = Random.Range(-1f,1f)*maxRandomSpeed;
		ay = Random.Range(-1f,1f)*maxRandomSpeed;
		az = Random.Range(-1f,1f)*maxRandomSpeed;
		elapsedTime = 0;
		delay = Random.Range (0,randomDelay);
	}
	
	// Update is called once per frame
	void Update () {
		float t = Time.deltaTime;
		elapsedTime += t;
		if(elapsedTime > delay){
			Init();
		}
		transform.Rotate(ax*t,ay*t,az*t);
			
	}
}
