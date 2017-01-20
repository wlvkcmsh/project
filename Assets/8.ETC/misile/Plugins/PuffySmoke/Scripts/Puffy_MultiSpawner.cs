using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Puffy_MultiSpawner : MonoBehaviour
{
	protected Transform _transform;
	public Puffy_Emitter emitter;
	private Puffy_Emitter _emitter;
	public List<Puffy_ParticleSpawner> spawnerList = new List<Puffy_ParticleSpawner> ();

	public GameObject prefab;

	void Awake ()
	{
		_transform = transform;
		if (!emitter)
			emitter = gameObject.GetComponent<Puffy_Emitter> ();
		if (emitter)
			emitter.Link(this);
	}

	// Add the specified spawner into the spawner pool list
	public bool AddSpawner(Puffy_ParticleSpawner spawner){
		spawner.emitter = emitter;

		if (spawner.multiSpawner && spawner.multiSpawner != this) {
			spawner.multiSpawner.RemoveSpawner(spawner);
		}

		if(!spawnerList.Contains(spawner)){
			spawnerList.Add (spawner);
			spawner.SetMultiSpawner(this);
			return true;
		}else{
			return false;
		}
	}

	// Delete the specified spawner from the spawner pool list
	public bool RemoveSpawner(Puffy_ParticleSpawner spawner){
		if (spawnerList.Contains (spawner)) {
			if(spawner.multiSpawner == this) spawner.UnsetMultiSpawner();
			return spawnerList.Remove (spawner);
		} else {
			return false;
		}
	}
	
	// create a new particles spawner
	// and add it to the spawners pool list
	public Puffy_ParticleSpawner CreateSpawner (Vector3 position, Vector3 direction = default(Vector3))
	{
		if(direction == Vector3.zero) direction = Vector3.forward;

		Puffy_ParticleSpawner spawner = GetNextFreeSpawner();
		if(spawner != null){
			spawner.Wake (position,direction);
			return spawner;
		}

		// no free instance has been found, create a new one
		GameObject go;
		if(prefab == null){
			go = new GameObject ();
		}else{
			go = Instantiate(prefab , Vector3.zero , Quaternion.identity) as GameObject;
		}
		return MakeSpawner (go, position, direction);
	}
	
	// Find an unused spawner to recycle
	public Puffy_ParticleSpawner GetNextFreeSpawner(){
		if(spawnerList.Count > 0){
			for(int i = 0; i < spawnerList.Count; i++){
				if(!spawnerList[i].gameObject.activeInHierarchy){
					return spawnerList[i];
				}
			}
		}
		
		return null;
	}
	
	// convert an existing gameobject as a new particles spawner
	// and add it to the spawners pool list
	public Puffy_ParticleSpawner MakeSpawner (GameObject gameObject, Vector3 position = default(Vector3), Vector3 direction = default(Vector3))
	{
		if(direction == Vector3.zero) direction = Vector3.forward;
		
		Puffy_ParticleSpawner spawner = gameObject.GetComponent<Puffy_ParticleSpawner> ();
		if (spawner == null){
			spawner = gameObject.AddComponent <Puffy_ParticleSpawner>() as Puffy_ParticleSpawner;
		}
		
		spawner.InitSpawnPoint (position, direction);
		
		AddSpawner (spawner);
		
		return spawner;
	}

	public void DoUpdate (float deltaTime)
	{
		
		if(!emitter) return;
		
		Vector3 spawnPosition, offset, offsetStep = Vector3.zero;
		Vector3 spawnDirection, dir, dirStep = Vector3.zero;
		Vector3 startDirection = emitter.transform.TransformDirection(emitter.startDirection).normalized;

		float spawnSpeed, age = 0f;

		int j, index, localSubCounter = 0;
		float step = 1f / emitter.spawnRate;
		float subParticlesRatio = emitter.subParticlesRatio;
		int subParticlesCounter = emitter.subParticlesCounter;
		int subParticlesCount = emitter.subParticlesCount;
		
		float ratio = deltaTime / step;
		
		int spawnCount = Mathf.FloorToInt (ratio);

		float lifeTime = emitter.lifeTime;
		float startSize = emitter.startSize;
		float endSize = emitter.endSize;
		
		Color startColor = emitter.startColor;
		Color endColor = emitter.endColor;
		
		Puffy_ParticleData particle;
		
		float maxParticlesDistance = emitter.maxParticlesDistance;
		float localRatio, localStep;
		int localSpawnCount;
		
		spawnSpeed = emitter.startSpeed;
		int spawnerCount = spawnerList.Count;
		int i;
		Puffy_ParticleSpawner spawner;
		//foreach (Puffy_ParticleSpawner spawner in spawnerList) {
		for(i=0;i<spawnerCount;i++){
			spawner = spawnerList[i];
			localSubCounter = subParticlesCounter;
			if(spawner){
				if (spawner.enabled && spawner.gameObject.activeInHierarchy) {
					
					localRatio = ratio;
					localSpawnCount = spawnCount;
					localStep = step;
					
					age = deltaTime;
					
					//age -= localStep; // TO CHECK
					spawner.DoUpdate();

					offset = spawner.spawnPosition - spawner.lastPosition;
					offsetStep = offset / localRatio;

					//Debug.Log ("DoUpdate in : "+deltaTime+" "+spawner.lastPosition.z+" -> "+spawner.spawnPosition.z+" / offset="+offset.magnitude+" / ratio="+localRatio);

					if (maxParticlesDistance > 0) {
						
						float magnitude = offsetStep.magnitude;
						
						if (magnitude > maxParticlesDistance) {
							localRatio = Mathf.Min (offset.magnitude / maxParticlesDistance,spawnCount * 100f);
							localSpawnCount = Mathf.FloorToInt (localRatio);
							
							//if (localSpawnCount < spawnCount * 100) {
								offsetStep = offset.normalized * maxParticlesDistance;
								localStep = (spawnCount * step) / localSpawnCount;
							/*
							} else {
								localRatio = ratio;
								localSpawnCount = spawnCount;
							}
							*/
						}
					}
					
					spawnPosition = spawner.lastPosition;

					if(spawner.useLocalDirection){
						dir = spawner.particleDirection - spawner.lastParticleDirection;
					}else{
						dir = startDirection - spawner.lastParticleDirection;
					}

					dirStep = dir / localRatio;
					spawnDirection = spawner.lastParticleDirection;
					particle = null;
//					Debug.Log ("start "+spawnDirection);

					for (j = 0; j < localSpawnCount; j++) {
						
						spawnPosition += offsetStep;
						spawnDirection += dirStep;

						//Debug.Log (j+") "+spawnDirection);

						index = emitter.SpawnParticle (spawnPosition, spawnDirection, spawnSpeed, lifeTime, startSize, endSize, startColor, endColor, age);
					
						age -= localStep;
						
						if(index > -1){
							particle = emitter.particles[index];
							
							if(spawner.colorGradient == null){
								particle.colorGradient = emitter.GetActiveGradient();
								
							}else if(spawner.colorGradient.enabled){
								particle.colorGradient = spawner.colorGradient;
								
							}else{
								particle.colorGradient = emitter.GetActiveGradient();
							}
							
							if (index >= 0 && subParticlesCount > 0) {
									
								if (localSubCounter < subParticlesCount) {
									
									particle.startLifetime *= subParticlesRatio;
									particle.endSize *= subParticlesRatio;
							
									if (emitter.debugIntermediate) {
										particle.startColor = Color.yellow;
										particle.endColor = Color.yellow;
									}
									
								} else {
									if (emitter.debugIntermediate) {
										particle.startColor = Color.magenta;
										particle.endColor = Color.magenta;
									}
									localSubCounter = 0;
								}
								
								localSubCounter++;
							}
						}else{
							break;
						}
					}
					
					spawner.lastPosition = spawnPosition;
					spawner.lastParticleDirection = spawnDirection;

					//Debug.Log ("DoUpdate out : "+deltaTime+" "+spawner.lastPosition.z+" -> "+spawner.spawnPosition.z+" / offset="+(spawner.lastPosition-spawner.spawnPosition).magnitude+" / ratio="+localRatio);
					
				}

			}
			emitter.subParticlesCounter = localSubCounter;
		}
	}

	
	void OnValidate(){
		
		if(_emitter != null && _emitter != emitter) _emitter.UnLink(this);
		if (emitter) emitter.Link(this);
		_emitter = emitter;
		
	}
	
	void OnDestroy(){
		if (emitter)
			emitter.UnLink(this);
	}

	void OnDrawGizmos(){
		
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube(transform.position,Vector3.one);
		Gizmos.color = new Color(0,1,1,0.5f);
		foreach (Puffy_ParticleSpawner spawner in spawnerList) {
			if(spawner) Gizmos.DrawLine(transform.position , spawner.transform.position);
		}
		
		if(emitter != null){
			Gizmos.color = new Color(1f,0.5f,0f,0.5f);
			Gizmos.DrawLine(transform.position , emitter.transform.position);
		}
	}

}