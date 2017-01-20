using UnityEngine;
using System.Collections;

public class RandomMove : MonoBehaviour {
	
	public int radius = 5;
	public int delay = 5;
	public float speed = 0.3f;
	
	private float timer = 0f;
	private Transform _transform;
	private Vector3 _destination;
	
	public bool moving = true;
	
	void Start () {
		_transform = transform;
		timer = delay+1;
	}
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.T)) moving = !moving;
		if(moving){
			timer += Time.deltaTime;
			
			if(timer > delay){
				_destination = _transform.position + Random.insideUnitSphere * radius;
				timer = 0;
			}else{
				_transform.Translate((_destination - _transform.position).normalized * speed * Time.deltaTime);
			}
		}
	}
}
