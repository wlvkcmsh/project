using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class Puffy_Emitter : MonoBehaviour
{
	public static bool skipThisFrame = false;
	public static bool globalFreeze = false;
	public Puffy_Renderer puffyRenderer;
	public bool autoEmit = true;
	public bool autoAssign = true;
	public string autoRendererName = "";
	public bool freezed = false;
	public float lifeTime = 5f;
	public float lifeTimeVariation = 0f;
	public Vector3 positionVariation = Vector3.zero;
	public Vector3 startDirection = Vector3.up;
	private Vector3 normalizedStartDirection = Vector3.up;
	public Vector3 startDirectionVariation = new Vector3 (0.01f, 0.01f, 0.01f);
	public float startSize = 0.5f;
	public float endSize = 2f;
	public float startSizeVariation = 0f;
	public float endSizeVariation = 0f;
	public float startSpeed = 1f;
	public float startSpeedVariation = 0f;
	public Color startColor = Color.white;
	public Color endColor = Color.white;
	public Color startColorVariation = Color.black;
	public Color endColorVariation = Color.black;
	public Puffy_Gradient colorGradient = null;
	public float luminosityEndTime = 1f;
	public float maxParticlesDistance = 0.0f;

	public bool warmUp = false;

	private float _deltaTimeAccumulation = 0;
	private byte skippedCount = 0;
	public enum colorModes
	{
		Basic,
		Mesh,
		Gradient
	}
	
	public colorModes colorMode = colorModes.Basic;
	public bool useLuminosity = false;
	public AnimationCurve luminosityCurve = new AnimationCurve (new Keyframe (0, 0), new Keyframe (1, 0));
	public bool autoResize = true;
	
	private ManualResetEvent[] doneEvents;

	public float spawnRate = 25;
	public bool useThread = true;
	public bool debugIntermediate = false;
	
	//public bool trailMode = true;
	public float trailStepDistance = 0.1f;
	public bool autoTrailStep = true;
	public float autoTrailStepFactor = 0.3f;
	public float autoTrailStepRatio = 0.5f;
	[System.NonSerialized]
	public List<Puffy_ParticleData> particles;
	[System.NonSerialized]
	public double debugTime = 0;
	[System.NonSerialized]
	public int particleTotal = 0;
	[System.NonSerialized]
	public int particleLive = 0;
	[System.NonSerialized]
	public List<int> particlesPointers;
	public int allowedParticleCount = 1024; // total number of allowed particles
	public int warmUpCount = 1024; //how many particles to warmup
	private int liveParticleCount = 0;
	private int _particleCount = 0;
	private Vector3 lastSpawnPosition = Vector3.zero;
	private Vector3 lastSpawnDirection = Vector3.zero;
	private float lastStartSpeed = 0f;
	private float _deltaTime;
	private int _coresCount = 1;
	private List<PointerGroup> deadGroups = new List<PointerGroup> ();
	private Transform _transform;
	private float elapsedAccumulation = 0;
	public int subParticlesCount = 0;
	public int subParticlesCounter = 0;
	public float subParticlesRatio = 0.5f;
	private bool wasStopped = false;
	public List<Puffy_ShapeSpawner> shapeSpawnerList = new List<Puffy_ShapeSpawner> ();
	public List<Puffy_MultiSpawner> multiSpawnerList = new List<Puffy_MultiSpawner> ();
	
	//public bool hasGradient = false;
	[HideInInspector]
	public bool cloudEmitter = false;
	[HideInInspector]
	public bool ready = false;

	private bool useSpacePartitionning = false;
	
	void Awake ()
	{
		//useThread = false;
		_transform = transform;

		lastSpawnPosition = _transform.position;
		
		_coresCount = SystemInfo.processorCount;
				
		particleTotal = 0;
		_particleCount = 0;
		particles = new List<Puffy_ParticleData> ();
		particlesPointers = new List<int> ();
		
		Clear ();
		
		colorGradient = GetComponent<Puffy_Gradient> ();
		
		if (colorMode == colorModes.Gradient && colorGradient == null) {
			colorGradient = (gameObject.AddComponent <Puffy_Gradient>() as Puffy_Gradient);
			//hasGradient = colorGradient.enabled;
		}
		
		doneEvents = new ManualResetEvent[_coresCount];
		for(int i=0;i<_coresCount;i++){
			doneEvents [i] = new ManualResetEvent (false);
		}

		if(warmUp) WarmUp(Mathf.Min(warmUpCount,allowedParticleCount));
	}
	
	void Start(){
		UnityEngine.Random.seed = 0;

		if (autoAssign) {
			//if (puffyRenderer == null) {
				Puffy_Renderer renderer = null;
				
				if(cloudEmitter){
					renderer = Puffy_Renderer.GetCloudRenderer();
					
				}else if (autoRendererName == "") {
					renderer = Puffy_Renderer.GetRenderer ();
				} else {
					renderer = Puffy_Renderer.GetRenderer (autoRendererName);
				}
				
				if (renderer == null) {
					renderer = Puffy_Renderer.GetRenderer ();
					if (renderer) {
						Debug.LogWarning ("Can't find a PuffyRenderer gameobject with the name '" + autoRendererName + "' , the first renderer found is picked instead");
					} else {
						Debug.LogWarning ("Can't find any PuffyRenderer");
					}
					
				}
				if (renderer)
					renderer.AddEmitter (this);
				puffyRenderer = renderer;
			//}
			
		} else if (puffyRenderer) {
			puffyRenderer.AddEmitter (this);
		}
		ready = true;
	}
	
	public void WarmUp(int count){
		int i;
//		double now = Time.realtimeSinceStartup;
		for (i=0; i<count; i++) {
			if(particlesPointers.Count <= i) particlesPointers.Add (i);
			particlesPointers [i] = i;
			
			if(particles.Count <= i) particles.Add (new Puffy_ParticleData (i));
			
			if (particles [i] == null)
				particles.Add (new Puffy_ParticleData (i));
			
			particles [i].Kill ();
		}
		_particleCount = particles.Count;
		particleTotal = _particleCount;
		
		liveParticleCount = 0;
		particleLive = 0;
//		Debug.Log ("Emitter warm up took "+((Time.realtimeSinceStartup-now)*1000).ToString("f3")+" ms");
	}
	
	public void Clear (bool reset = false)
	{
		int i;
			
		int count = Mathf.Min (allowedParticleCount, particles.Count);
		
		for (i=0; i<count; i++) {
			particlesPointers [i] = i;
			if (particles [i] == null)
				particles.Add (new Puffy_ParticleData (i));
			particles [i].Kill ();
		}
				
		liveParticleCount = 0;
		particleLive = 0;
	}
	
	bool waitingForLastParticle = false;

	public void Kill ()
	{
		waitingForLastParticle = true;
	}
	
	public void Resurrect ()
	{
		waitingForLastParticle = false;
		enabled = true;
	}
	
	bool doFreeze = false;
	
	// link any gameobject to this emitter and make it a particle spawn point, turn it into a multi-emitter if needed
	public Puffy_ParticleSpawner Link(GameObject gameObject, Vector3 direction){
		if(gameObject != null){
			Puffy_MultiSpawner ms;
			
			if(multiSpawnerList.Count == 0){
				ms = this.gameObject.AddComponent<Puffy_MultiSpawner>();	
				Link (ms);
			}else{
				ms = multiSpawnerList[0];	
			}
			
			return ms.MakeSpawner(gameObject,gameObject.transform.position,direction);
		}else{
			return null;
		}
	}
	
	// link an existing spawn point to this emitter by adding it to the first multispawner already linked to this emitter
	// if not multispawner exists, one will be created
	public bool Link(Puffy_ParticleSpawner spawner){
		if(spawner != null){
			Puffy_MultiSpawner ms;
			
			if(multiSpawnerList.Count == 0){
				ms = this.gameObject.AddComponent<Puffy_MultiSpawner>();	
				Link (ms);
			}else{
				ms = multiSpawnerList[0];	
			}
			
			ms.AddSpawner(spawner);
			return true;
		}else{
			return false;
		}
	}
	
	// link a multi spawner to this emitter
	public bool Link (Puffy_MultiSpawner multiSpawner)
	{
		if (multiSpawnerList.IndexOf (multiSpawner) == -1){
			multiSpawner.emitter = this;
			multiSpawnerList.Add (multiSpawner);
			return true;
		}else{
			return false;
		}
	}
	
	public void UnLink (Puffy_MultiSpawner multiSpawner)
	{
		multiSpawnerList.Remove (multiSpawner);
	}
	
	public bool Link (Puffy_ShapeSpawner shapeSpawner)
	{
		if (shapeSpawnerList.IndexOf (shapeSpawner) == -1){
			shapeSpawner.emitter = this;
			shapeSpawnerList.Add (shapeSpawner);
			return true;
		}else{
			return false;
		}
	}
	
	public void UnLink (Puffy_ShapeSpawner shapeSpawner)
	{
		shapeSpawner.emitter = null;
		shapeSpawnerList.Remove (shapeSpawner);
	}
	
	public Puffy_Gradient GetActiveGradient ()
	{
		if (colorGradient != null) {
			if (colorGradient.enabled && colorMode == colorModes.Gradient)
				return colorGradient;
		}
		
		return null;
	}
	
	void LateUpdate ()
	{
		
		//if(colorGradient != null) hasGradient = colorGradient.enabled;
		
		normalizedStartDirection = startDirection.normalized;
		
		if (!doFreeze) {
			debugTime = Time.realtimeSinceStartup;

			if (!waitingForLastParticle) {
				
				
				if (autoEmit && Time.timeScale > 0) {	
					float elapsed = Time.deltaTime;
					float step = 1f / spawnRate;
					int activeMultiSpawners = 0;
					int activeShapeSpawners = 0;
					
					elapsedAccumulation += elapsed;
					
					if (elapsedAccumulation >= step) {
						
						float ratio = elapsedAccumulation / step;
						int spawnCount = Mathf.FloorToInt (ratio);
												
						float difference = step * spawnCount;
						int spawnerCount = 0;
						int i;
						if (multiSpawnerList.Count > 0) {
							spawnerCount = multiSpawnerList.Count;
							
							for(i=0;i<spawnerCount;i++){
                                if (multiSpawnerList == null)
                                {
                                    multiSpawnerList.RemoveAt(i);
                                    i--;
                                    spawnerCount--;
                                }
								else if (multiSpawnerList[i].enabled) {
									multiSpawnerList[i].DoUpdate (elapsedAccumulation);
									activeMultiSpawners++;
								}
							}
						}
					
						if (shapeSpawnerList.Count > 0) {
							spawnerCount = shapeSpawnerList.Count;
							for(i=0;i<spawnerCount;i++){
								if(shapeSpawnerList == null){
									shapeSpawnerList.RemoveAt(i);
									i--;
									spawnerCount--;

								}else if (shapeSpawnerList[i].enabled) {
									if (wasStopped)
										shapeSpawnerList[i].Init ();
									shapeSpawnerList[i].DoUpdate (elapsedAccumulation);
									activeShapeSpawners++;
								}
							}
						}
						
						if (activeShapeSpawners == 0 && activeMultiSpawners == 0) {
							Vector3 spawnPosition, offset, offsetStep = Vector3.zero;
							Vector3 spawnDirection, dir, dirStep = Vector3.zero;
							float spawnSpeed, spd, spdStep = 0f;
							
							int j, index = 0;
							
							Vector3 transformedPosition = _transform.position;
							Vector3 transformedDirection = _transform.TransformDirection (normalizedStartDirection);
							
							Puffy_ParticleData particle;
							
							if (wasStopped) {
								lastSpawnPosition = transformedPosition;
								lastSpawnDirection = transformedDirection;
								lastStartSpeed = startSpeed;
							}
							
							float localRatio, localStep;
							int localSpawnCount;
							
							float age = elapsedAccumulation;
							
							
							
							localRatio = ratio;
							localSpawnCount = spawnCount;
							localStep = step;
							
							//age -= localStep; // TO CHECK
							
							offset = transformedPosition - lastSpawnPosition;
							offsetStep = offset / localRatio;
							
							if (maxParticlesDistance > 0) {
								float magnitude = offsetStep.magnitude;
								if (magnitude > maxParticlesDistance) {
									localRatio = offset.magnitude / maxParticlesDistance;
									localSpawnCount = Mathf.FloorToInt (localRatio);
									if (localSpawnCount < spawnCount * 10) {
										offsetStep = offset.normalized * maxParticlesDistance;
										localStep = (spawnCount * step) / localSpawnCount;
									} else {
										localRatio = ratio;
										localSpawnCount = spawnCount;
									}
								}
							}
							
							spawnPosition = lastSpawnPosition;
							
							dir = transformedDirection - lastSpawnDirection;
							dirStep = dir / localRatio;
							spawnDirection = lastSpawnDirection;
							
							spd = startSpeed - lastStartSpeed;
							spdStep = spd / localRatio;
							spawnSpeed = lastStartSpeed;
							
							for (j=0; j<localSpawnCount; j++) {
								spawnPosition += offsetStep;
								spawnDirection += dirStep;
								spawnSpeed += spdStep;
								
								index = SpawnParticle (spawnPosition, spawnDirection, spawnSpeed, lifeTime, startSize, endSize, startColor, endColor, age);
								
								if(index < 0) break;
								
								particle = particles [index];
								if (colorMode == colorModes.Gradient)
									particle.colorGradient = colorGradient;
								
								age -= localStep;
								
								if (index >= 0 && subParticlesCount > 0) {
									if (subParticlesCounter < subParticlesCount) {

										particle.startLifetime *= subParticlesRatio;
										particle.endSize *= subParticlesRatio;
									
										if (debugIntermediate) {
											particle.startColor = Color.yellow;
											particle.endColor = Color.yellow;
										}
									} else {
										if (debugIntermediate) {
											particle.startColor = Color.magenta;
											particle.endColor = Color.magenta;
										}
										subParticlesCounter = 0;
									}
									subParticlesCounter++;
								}
							}
							lastSpawnPosition = spawnPosition;
							lastSpawnDirection = spawnDirection;
							lastStartSpeed = spawnSpeed;
						}
					
						elapsedAccumulation -= difference;
						
					}
		
					wasStopped = false;
				} else {
										
					wasStopped = true;
				}
				
			} else if (waitingForLastParticle && liveParticleCount == 0) {
				// kill only when no particle is left
				Clear ();
				enabled = false;
				debugTime = Time.realtimeSinceStartup - debugTime;
				return;
			}
			
			updatetimer = Time.realtimeSinceStartup;
			UpdateParticles ();
			updatetimer = Time.realtimeSinceStartup - updatetimer;

			debugTime = Time.realtimeSinceStartup - debugTime;
		} else {
			
			wasStopped = true;
		}
		if(debugTime > 0.0001f && cloudEmitter) skipThisFrame = true;

		doFreeze = (freezed || globalFreeze);
	}

//	private int updateStep = -1;
	private double updatetimer = 0;

	private double cleantimer = 0;

	private bool debugDeads = false;
	private PointerComparer indexComparer = new PointerComparer ();

	private struct ParticlesUpdateData
	{
		public int index;
		public int start;
		public int end;
	}

	private void Threaded_UpdateParticlesTask(System.Object stateInfo)
	{
		ParticlesUpdateData data = (ParticlesUpdateData)stateInfo;
		UpdateParticlesTask (data.index,data.start,data.end);
		doneEvents [data.index].Set ();
	}

	void UpdateParticles ()
	{
		
		if (liveParticleCount > 0) {

			if(skipThisFrame && skippedCount<1){
				_deltaTimeAccumulation += Time.deltaTime;
				skippedCount++;
				return;
			}

			while (deadGroups.Count<_coresCount) {
				deadGroups.Add (new PointerGroup ());					
			}

			_deltaTime = _deltaTimeAccumulation + Time.deltaTime;
			_deltaTimeAccumulation = 0;
			skippedCount = 0;
			skipThisFrame = false;

			int count;
			int i;
			int j;

			// don't use threads is too few particles are alive, or if there is only 1 core, or if this option is simply disabled
			if (liveParticleCount < _coresCount * 1000 || _coresCount == 1 || !useThread) {
				UpdateParticlesTask (0, 0, liveParticleCount);
			} else {
				
				int stp = Mathf.CeilToInt ((float)liveParticleCount / _coresCount);

				j = 0;
				for (i = 0; i < liveParticleCount; i += stp) {
					ParticlesUpdateData dat = new ParticlesUpdateData();
					dat.end = Mathf.Min (i + stp, liveParticleCount);;
					dat.start = i;
					dat.index = j;

					doneEvents[j].Reset();
					System.Threading.ThreadPool.QueueUserWorkItem (new System.Threading.WaitCallback (this.Threaded_UpdateParticlesTask), dat);

					j++;
				}

				WaitHandle.WaitAll (doneEvents);
			}
			
			
			int groupCount = deadGroups.Count;
			int g;
			int deadCount = 0;
			for (g = 0; g < groupCount; g++) {
				deadCount += deadGroups [g].count;
			}
			
			
			
			if (deadCount > 0) {
				debugstr = "";
				
				int index;
				int pointer;
				PointerGroup grp;
				
				if (debugDeads) {
					Debug.Log ("------------------------ deadCount=" + deadCount);
					for (g = 0; g < groupCount; g++) {
						grp = deadGroups [g];
						count = grp.count;
						
						for (i = 0; i < count; i++) {
							pointer = grp.pointers [i];
							index = particlesPointers [pointer];
							
							debugstr += index + ",";
							globalFreeze = true;
						}
					}
				}
				
				string str = "";
				int po;
				
				if (debugDeads) {
										
					str = "";
					for (po=0; po<particlesPointers.Count; po++) {
						if (po == liveParticleCount) {
							str += "[" + particlesPointers [po] + "], ";
						} else {
							str += particlesPointers [po] + ", ";
						}
					}
					Debug.Log ("source = " + str);
				}
				
				for (g = 0; g < groupCount; g++) {
					grp = deadGroups [g];
					count = grp.count;
					
					// indices must be sorted to be removed in the right order
					grp.pointers.Sort (0, count, indexComparer);
					
					if (debugDeads) {
						str = "";
						for (po=0; po<count; po++) {
							
							str += grp.pointers [po] + ", ";
							
						}
						if (str != "")
							Debug.Log ("pointers to kill = " + str);
						
						str = "";
						for (po=0; po<count; po++) {
							
							str += particlesPointers [grp.pointers [po]] + "(" + particles [particlesPointers [grp.pointers [po]]].ageRatio.ToString ("f2") + particles [particlesPointers [grp.pointers [po]]].dead + "), ";
							
						}
						if (str != "")
							Debug.Log ("to kill = " + str);
					}
				}
				
				int maxi = int.MinValue;
				int grpIndex = -1;
				while (deadCount > 0) {
					
					// find the next highest pointer index to remove
					maxi = int.MinValue;
					for (g = 0; g < groupCount; g++) {
						grp = deadGroups [g];
						if (grp.count > 0) {
							if (grp.pointers [grp.count - 1] > maxi) {
								maxi = grp.count - 1;
								grpIndex = g;
							}
						}
					}
					
					if (grpIndex != -1) {
						i = maxi;
						grp = deadGroups [grpIndex];
						grp.count--;
						
						pointer = grp.pointers [i];
						index = particlesPointers [pointer];
						
						liveParticleCount--;
												
						if (particles [index].killed) {
							// this should never appears if particles are killed in the right order
//							Debug.Log (index + " already KILLED !!!!");	
						} else {
							particles [index].Kill ();
						}
						
						particlesPointers [pointer] = particlesPointers [liveParticleCount];
						particlesPointers [liveParticleCount] = index;
						
						if (debugDeads) {
							str = "";
							for (po=0; po<particlesPointers.Count; po++) {
								if (po == liveParticleCount) {
									str += "[" + particlesPointers [po] + "], ";
								} else {
									str += particlesPointers [po] + ", ";
								}
							}
							Debug.Log ("result = " + str);
						}
					}
					
					deadCount--;
				}
				grp = null;
				particleLive = liveParticleCount;
			}
			
			particleTotal = _particleCount;
		}

	}

	private string debugstr = "";
	
	void OnDrawGizmos ()
	{

		if (multiSpawnerList.Count + shapeSpawnerList.Count > 0) {
			Gizmos.color = new Color (1f, 0.5f, 0f, 0.5f);
		} else {
			Gizmos.color = new Color (1f, 0.5f, 0f, 1f);
		}
		Gizmos.DrawWireSphere (transform.position, 0.4f);

		if(startDirection == Vector3.zero){
			Gizmos.DrawWireCube (transform.position,Vector3.one * 0.05f);
		}else{
			Vector3 dir = transform.TransformDirection (startDirection);
			Gizmos.DrawLine (transform.position, transform.position + dir.normalized * 2);
		}
			
		// draw gizmos if no renderer is assigned
		if (!puffyRenderer) {
			Color c;
			for (int i=0; i<liveParticleCount; i++) {
				c = particles [particlesPointers [i]].color;
				c.a *= (1f - particles [particlesPointers [i]].ageRatio);
				Gizmos.color = c;
				Gizmos.DrawWireSphere (particles [particlesPointers [i]].position, particles [particlesPointers [i]].size * 0.25f);
			}
		}
		/*
		GUIStyle stl = new GUIStyle ();
		stl.normal.textColor = Color.white;
		string str = "";
		for (int i=0; i<liveParticleCount; i++) {
			str = particlesPointers [i].ToString();

			Handles.Label(particles [particlesPointers [i]].position,str,stl);
		}
		*/

	}
	
	void OnGUI ()
	{
		
		if (puffyRenderer != null && useSpacePartitionning) {
			if (puffyRenderer.debug) {
				string str = "";
				str += "update time: " + (updatetimer * 1000).ToString ("f2") + "\n";
				str += "clean time: " + (cleantimer * 1000).ToString ("f2") + "\n";

				GUI.Label (new Rect (200, 30, 200, 120), str);

			}
		}
		
		//Puffy_SpacePartitionData.PrintDebug();
		//GUI.Label(new Rect(0,30,200,20),"space time: "+(spacetimer*1000).ToString("f2")+" x"+spacecounter);
		
		// draw particles indices
		/*
			Vector3 pos,screenPos;
			Rect r = new Rect(0,0,50,20);
			Camera cam = Camera.main.camera;
			
			List<int> tmp = new List<int>();
			float age;
			int i;
			GUI.color = Color.white;
			for(i=0;i<liveParticleCount;i++){
				age = particles[particlesPointers[i]].lifetime;
				pos = particles[particlesPointers[i]].position;
				screenPos = cam.WorldToScreenPoint(pos);
				r.y = (Screen.height - screenPos.y);
				r.x = screenPos.x;
				GUI.Label(r,particles[particlesPointers[i]].index.ToString());
			}
		*/

	}
	
	void UpdateParticlesTask (int groupIndex, int start, int end)
	{
		
		if (end > start && end > 0) {
			
			Puffy_ParticleData p = null;
			PointerGroup localGroup = deadGroups [groupIndex];
			int total = localGroup.pointers.Count;
			int index = 0;
			
			for (int pointer = start; pointer < end; pointer++) {
	
				p = particles [particlesPointers [pointer]];
				
				// do particle update
				p.Update (_deltaTime, useSpacePartitionning);
				
				
				// if the particle is dead, add it to the group of dead particles assigned to this thread
				if (p.dead) {
					if (index >= total) {
						localGroup.pointers.Add (pointer);
						total ++;
//						Debug.Log ("add pointer to kill at ("+index+") id pointer="+pointer+" id particle="+particlesPointers[pointer]);
					} else {
						localGroup.pointers [index] = pointer;
//						Debug.Log ("upd pointer to kill at ("+index+") id pointer="+pointer+" id particle="+particlesPointers[pointer]);
					}
					
					index ++;
					
				}
			}
			localGroup.count = index;
			localGroup = null;
		}
	}
	
	public void KillParticle (int index)
	{
		particles [index].Kill ();
	}
	
	private class PointerComparer : IComparer<int>
	{
		public int Compare (int a, int b)
		{
			return (a > b) ? 1 : -1;
		}
	}

	public int SpawnParticle (Vector3 start_position, Vector3 start_direction, float age = 0f)
	{
		return SpawnParticle (start_position, start_direction, startSpeed, lifeTime, startSize, endSize, startColor, endColor, age);
	}
	
	public int SpawnParticle (Vector3 start_position, Vector3 start_direction, float age = 0f, float start_lifeTime = 1f)
	{
		return SpawnParticle (start_position, start_direction, startSpeed, start_lifeTime, startSize, endSize, startColor, endColor, age);
	}
	
	// create one particle
	public int SpawnParticle (Vector3 start_position, Vector3 start_direction, float start_speed, float start_lifetime, float start_size, float end_size, Color start_color, Color end_color, float age = 0)
	{
		
		if (autoResize && liveParticleCount >= _particleCount) {
			
			particles.Add (new Puffy_ParticleData (liveParticleCount));
			particlesPointers.Add (liveParticleCount);
			_particleCount++;
			
		} else if (liveParticleCount >= particles.Count && liveParticleCount < allowedParticleCount) {
			
			particles.Add (new Puffy_ParticleData (liveParticleCount));
			particlesPointers.Add (liveParticleCount);
			_particleCount++;

		} else if (!autoResize && liveParticleCount >= allowedParticleCount) {
			
			return -1;
		}
		
		
		if (liveParticleCount < _particleCount && particlesPointers != null) {
			int index = particlesPointers [liveParticleCount];
	
			liveParticleCount++;
			
			particleLive = liveParticleCount;

			if (lifeTimeVariation != 0)
				start_lifetime += UnityEngine.Random.Range (-lifeTimeVariation, lifeTimeVariation);
			if (startSizeVariation != 0)
				start_size += UnityEngine.Random.Range (-startSizeVariation, startSizeVariation);
			if (endSizeVariation != 0)
				end_size += UnityEngine.Random.Range (-endSizeVariation, endSizeVariation);
			if (startColorVariation != Color.black) {
				start_color.r += UnityEngine.Random.Range (-startColorVariation.r, startColorVariation.r);
				start_color.g += UnityEngine.Random.Range (-startColorVariation.g, startColorVariation.g);
				start_color.b += UnityEngine.Random.Range (-startColorVariation.b, startColorVariation.b);
				
				start_color.r = Mathf.Clamp01 (start_color.r);
				start_color.g = Mathf.Clamp01 (start_color.g);
				start_color.b = Mathf.Clamp01 (start_color.b);
				
			}
			if (endColorVariation != Color.black) {
				end_color.r += UnityEngine.Random.Range (-endColorVariation.r, endColorVariation.r);
				end_color.g += UnityEngine.Random.Range (-endColorVariation.g, endColorVariation.g);
				end_color.b += UnityEngine.Random.Range (-endColorVariation.b, endColorVariation.b);
				
				end_color.r = Mathf.Clamp01 (end_color.r);
				end_color.g = Mathf.Clamp01 (end_color.g);
				end_color.b = Mathf.Clamp01 (end_color.b);
			}
			
			if (startSpeedVariation != 0)
				start_speed += UnityEngine.Random.Range (-startSpeedVariation, startSpeedVariation);
			
			if (startDirectionVariation != Vector3.zero) {
				start_direction.x += UnityEngine.Random.Range (-startDirectionVariation.x, startDirectionVariation.x);
				start_direction.y += UnityEngine.Random.Range (-startDirectionVariation.y, startDirectionVariation.y);
				start_direction.z += UnityEngine.Random.Range (-startDirectionVariation.z, startDirectionVariation.z);
			}
			
			
			if (positionVariation != Vector3.zero) {
				
				start_position.x += UnityEngine.Random.Range (-positionVariation.x, positionVariation.x);
				start_position.y += UnityEngine.Random.Range (-positionVariation.y, positionVariation.y);
				start_position.z += UnityEngine.Random.Range (-positionVariation.z, positionVariation.z);
				
			}
			
			// uniform size
			//end_size = (endSize + endSizeVariation) * (start_lifetime/(lifeTime+lifeTimeVariation));
			
			/*
			start_position.x = debugposition;
			debugposition+=0.5f;
			if(debugposition > 10) debugposition = -10;
			*/
			if (colorMode == colorModes.Gradient) {
				particles [index].Spawn (start_position, start_direction, start_speed, start_lifetime, start_size, end_size, age);
			} else {
				particles [index].Spawn (start_position, start_direction, start_speed, start_lifetime, start_size, end_size, start_color, end_color, age);
			}
						
			return index;
		}
		
		return -1;
	}
	//private float debugposition = -10;

	public int FastSpawn(){
		if (autoResize && liveParticleCount >= _particleCount) {
			
			particles.Add (new Puffy_ParticleData (liveParticleCount));
			particlesPointers.Add (liveParticleCount);
			_particleCount++;
			
		} else if (liveParticleCount >= particles.Count && liveParticleCount < allowedParticleCount) {
			
			particles.Add (new Puffy_ParticleData (liveParticleCount));
			particlesPointers.Add (liveParticleCount);
			_particleCount++;
			
		} else if (!autoResize && liveParticleCount >= allowedParticleCount) {
			
			return -1;
		}

		if (liveParticleCount < _particleCount && particlesPointers != null) {
			int index = particlesPointers [liveParticleCount];
			
			liveParticleCount++;
			
			particleLive = liveParticleCount;

			particles [index].Spawn();
			return index;
		}

		return -1;
	}

	public void SpawnRow (Vector3 row_start_position, Vector3 row_end_position, Vector3 velocityOffset, float intermediateRatio = -1f)
	{
		if (intermediateRatio < 0)
			intermediateRatio = autoTrailStepRatio;
		SpawnRow (row_start_position, row_end_position, trailStepDistance, normalizedStartDirection + velocityOffset, startSpeed, lifeTime, startSize, endSize, startColor, endColor, intermediateRatio);
	}
	
	// create a row of particles
	public void SpawnRow (Vector3 row_start_position, Vector3 row_end_position, float stepDistance, Vector3 start_direction, float start_speed, float start_lifetime, float start_size, float end_size, Color start_color, Color end_color, float intermediateRatio = -1f)
	{
		
		if (intermediateRatio < 0)
			intermediateRatio = autoTrailStepRatio;
		
		Vector3 dir = row_end_position - row_start_position;
		float distance = dir.magnitude;
		int count;
		
		if (autoTrailStep) {
			count = Mathf.FloorToInt (distance / (start_size * autoTrailStepFactor));
		} else {
			count = Mathf.FloorToInt (distance / stepDistance);
		}
		
		if (count < 2 || intermediateRatio == 0) {
			SpawnParticle (row_end_position, start_direction, start_speed, start_lifetime, start_size, end_size, start_color, end_color);
		} else {
			int i;
			float stepDist = distance / count;
			dir = dir.normalized * stepDist;
			int index;
			float age = 0;
			float stepTime = _deltaTime / count;
			
			for (i=0; i<count; i++) {
				
				index = SpawnParticle (row_end_position, start_direction, start_speed, start_lifetime, start_size, end_size, start_color, end_color, age);
			
				if (index > -1) {
					if (i > 0 && count > 1) {
						//intermediate particles are smaller and die sooner, to keep performances
						particles [index].startLifetime *= intermediateRatio;
						particles [index].endSize *= intermediateRatio;
						
						if (debugIntermediate) {
							particles [index].startColor = Color.yellow;
							particles [index].endColor = Color.yellow;
						}
					} else {
						if (debugIntermediate) {
							particles [index].startColor = Color.magenta;
							particles [index].endColor = Color.magenta;
						}
					}
				}else{
					break;
				}
				age += stepTime;
				row_end_position -= dir;
			}
			
		}
	
	}
	
	class PointerGroup
	{
		public List<int> pointers = new List<int> ();
		public int count = 0;
	}
	

}


public struct Puffy_SpawnData{
	public float spawnRate; // particlres/second
	public float maxParticlesDistance; // max gap
	public Vector3 positionVariation;
	public Vector3 direction;
	public Vector3 directionVariation;
	public float startLifeTime;
	public float lifeTimeVariation;
	public float startSpeed;
	public float startSpeedVariation;
	public float startSize;
	public float startSizeVariation;
	public float endSize;
	public float endSizeVariation;
	public Color startColor;
	public Color startColorVariation;
	public Color endColor;
	public Color endColorVariation;
}

public class Puffy_ParticleData
{
	
	public Vector3 position = Vector3.zero;
	public Vector3 direction = Vector3.up;
	public float randomSeed = 0;
	public float startLifetime = 0f;
	public float lifetime = 0f;
	public Color startColor = Color.white;
	public Color color = Color.white;
	public Color endColor = Color.white;
	public float startSize = 0f;
	public float size = 0f;
	public float endSize = 1;
	public float alphaMultiplier = 1f;
	public float ageRatio = 0f;
	public float speed = 0f;
	public Puffy_Gradient colorGradient = null;
	private bool justEmitted = true;
	public bool dead = true;
	public bool killed = true;
	public int index = 0;


	public Puffy_ParticleData (int index)
	{
		this.index = index;
		//this.partitionKey = new Puffy_SpacePartitionKey (0, 0, 0);
	}

	public void Spawn(){
		colorGradient = null;
		
		alphaMultiplier = 1f;

		dead = false;
		killed = false;

		justEmitted = true;

	}

	public void Spawn (Vector3 start_position, Vector3 start_direction, float start_speed, float start_lifetime, float start_size, float end_size, Color start_color, Color end_color, float start_age = 0f)
	{
		
		speed = start_speed;
		
		position = start_position;
		direction = start_direction;
		
		startSize = start_size;
		endSize = end_size;
		
		startColor = start_color;
		endColor = end_color;
				
		lifetime = start_age;
		
		startLifetime = start_lifetime;
		
		color = start_color;
		
		colorGradient = null;
		
		alphaMultiplier = 1f;
		
		randomSeed = UnityEngine.Random.Range (0f, 0.5f);
		
		dead = false;
		killed = false;

		justEmitted = true;

	}
	
	public void Spawn (Vector3 start_position, Vector3 start_direction, float start_speed, float start_lifetime, float start_size, float end_size, float start_age = 0f)
	{
		
		speed = start_speed;
		
		position = start_position;
		direction = start_direction;
		
		startSize = start_size;
		endSize = end_size;
				
		lifetime = start_age;
		startLifetime = start_lifetime;
		
		color = startColor;
		
		colorGradient = null;
		
		alphaMultiplier = 1f;
		
		randomSeed = UnityEngine.Random.Range (0f, 0.5f);
		
		dead = false;
		killed = false;

		justEmitted = true;

	}
	
	public void Update (float deltaTime, bool useSpacePartitionning)
	{
	
		if (!justEmitted)
			lifetime += deltaTime;
		
		if (startLifetime > 0) {
			dead = lifetime > startLifetime;
			
			if (!dead) {
				ageRatio = lifetime / startLifetime;
				size = Mathf.Lerp (startSize, endSize, ageRatio);
				
				if (speed != 0) {
					position += direction * speed * deltaTime;

				}
				
				if (UnityEngine.Object.Equals (colorGradient, null))
					color = Color.Lerp (startColor, endColor, ageRatio);
			}
		} else {
			ageRatio = 0;
		}
			
		justEmitted = false;
		
	}
	
	public void Kill ()
	{
		dead = true;
		lifetime = 0;
		startLifetime = 0;
		size = 0;
		killed = true;
	}
	
	
}