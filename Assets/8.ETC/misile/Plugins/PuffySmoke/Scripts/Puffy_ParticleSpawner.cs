using UnityEngine;
using System.Collections;

public class Puffy_ParticleSpawner : MonoBehaviour {

	protected Transform _transform = null;

	public Puffy_MultiSpawner multiSpawner;
	private Puffy_MultiSpawner _multiSpawner;

	[HideInInspector]
	public Puffy_Emitter emitter;
	
	[HideInInspector]
	public Puffy_Gradient colorGradient;
	
	[HideInInspector]
	public Vector3 particleDirection = Vector3.zero;
	
	[HideInInspector]
	public Vector3 spawnPosition = Vector3.zero;


	public Vector3 localParticleDirection = Vector3.zero;
	public bool useLocalDirection = false;

	public Vector3 localParticleOffset = Vector3.zero;
	
	[HideInInspector]
	public Vector3 lastPosition = Vector3.zero;
	
	[HideInInspector]
	public Vector3 lastParticleDirection = Vector3.zero;

	private Vector3 previousDirection;

	void Awake(){
		_transform = transform;

		spawnPosition = _transform.position + _transform.TransformDirection(localParticleOffset);
		lastPosition = spawnPosition;
		lastParticleDirection = particleDirection;
	}

	void OnValidate(){

		if (multiSpawner == null && _multiSpawner != null) {
			_multiSpawner.RemoveSpawner(this);
		}
		if(multiSpawner && multiSpawner != _multiSpawner) multiSpawner.AddSpawner (this);

		_multiSpawner = multiSpawner;
	}

	public void UnsetMultiSpawner(){
		if(multiSpawner) multiSpawner.RemoveSpawner(this);
		multiSpawner = null;
		_multiSpawner = null;
	}

	public void SetMultiSpawner(Puffy_MultiSpawner m){
		multiSpawner = m;
		if (multiSpawner == null && _multiSpawner != null) {
			_multiSpawner.RemoveSpawner(this);
		}

		if(multiSpawner && multiSpawner != _multiSpawner) multiSpawner.AddSpawner (this);
		_multiSpawner = multiSpawner;
	}

	void Start(){
		colorGradient = GetComponent<Puffy_Gradient>();
	}
	
	public void InitSpawnPoint(Vector3 position, Vector3 direction = default(Vector3)){

		if(_transform == null) _transform = transform;
		if(direction == Vector3.zero) direction = Vector3.forward;

		_transform.position = position;
		_transform.forward = direction;

		spawnPosition = position + _transform.TransformDirection(localParticleOffset);
		lastPosition = spawnPosition;

		particleDirection = localParticleDirection;
		if(!useLocalDirection && emitter != null) {
			particleDirection = emitter.transform.TransformDirection(emitter.startDirection).normalized;
		}else{
			particleDirection = this.transform.TransformDirection(particleDirection).normalized;
		}
		lastParticleDirection = particleDirection;

		//Debug.Log ("InitSpawnPoint : "+this.lastPosition+" -> "+this.spawnPosition+" "+(this.spawnPosition-this.lastPosition)+" dir="+particleDirection);
	}

	//maybe obsolete
	public void UpdateSpawnPoint(Vector3 position){
		spawnPosition = position;
	}

	//maybe obsolete
	public void UpdateSpawnPoint(Vector3 position, Vector3 direction){
		spawnPosition = position;
		particleDirection = direction.normalized;
	}

	public void DoUpdate(){

		spawnPosition = _transform.position + _transform.TransformDirection(localParticleOffset);

		if(useLocalDirection){
			particleDirection = _transform.TransformDirection(localParticleDirection).normalized;
		}



		//Debug.Log ("Update : "+this.lastPosition.z+" -> "+this.spawnPosition.z+" / offset="+(this.spawnPosition-this.lastPosition).magnitude);
	}


	// Disable this spawn point
	public void Sleep ()
	{

		gameObject.SetActive(false);
	}

	// Reactivate this spawn point
	public void Wake (Vector3 position, Vector3 direction = default(Vector3))
	{
		gameObject.SetActive(true);
		InitSpawnPoint(position,direction);
	}

	void OnDestroy(){
		multiSpawner = null;
		if(_multiSpawner != null){
			_multiSpawner.RemoveSpawner(this);
		}
	}

	void OnDrawGizmos(){
		if(_transform == null) _transform = transform;
		Vector3 dir = Vector3.zero;

		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(_transform.position,0.25f);

		if(useLocalDirection){
			dir = _transform.TransformDirection(localParticleDirection).normalized;
		}else if(emitter != null){
			Gizmos.color = new Color(0,1,1,0.5f);
			dir = emitter.transform.TransformDirection(emitter.startDirection).normalized;
		}

		Vector3 pos = _transform.position + _transform.TransformDirection(localParticleOffset);
		Gizmos.DrawWireCube(pos,Vector3.one * 0.05f);

		if(dir != Vector3.zero){
			Gizmos.DrawLine(pos , pos + dir*2);
		}

		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(lastPosition,Vector3.one * 0.06f);

		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(spawnPosition,Vector3.one * 0.07f);
	}
	
}
