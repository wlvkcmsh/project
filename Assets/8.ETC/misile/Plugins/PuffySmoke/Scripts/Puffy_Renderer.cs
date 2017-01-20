using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Puffy_Renderer : MonoBehaviour
{
	public static List<Puffy_Renderer> instances = new List<Puffy_Renderer> ();
	public static int particlesPerCore_ChunkSize = 16384;

	public static void ToggleDebug ()
	{
		for (int i = 0; i < instances.Count; i++)
			instances [i].debug = !instances [i].debug;
	}

	public enum PassModeType
	{
		Auto,
		One,
		Multiple
	}


	public enum coreslevels
	{
		//Minimum = 1024,
		//Low = 2048,
		Medium = 4096,
		High = 8192,
		Maximum = 16384
	}

	// maximum must be 16200 instead of 16384
	// to be safe with mesh max vertices allocation
	public enum meshlevels
	{
		Minimum = 1024,
		Low = 2048,
		Medium = 4096,
		High = 8192,
		Maximum = 16200
	}

	public enum debugModes
	{
		Simple = 1,
		Advanced = 2
	}
	public bool radixSort = true;
	private bool useTangentShader;
	public int subMeshCount = 4;
	public bool warmUp = false;
	public int warmUpCount = 1;
	[HideInInspector]
	public bool
	perf_foldout = true;
	[HideInInspector]
	public bool
	aspect_foldout = true;
	[HideInInspector]
	public bool
	visibility_foldout = true;
	public coreslevels coresSetup = coreslevels.Maximum;
	public meshlevels meshSetup = meshlevels.Medium;
	private string debugString = "";
	public bool debug = false;
	public bool render = true;
	public debugModes debugMode = debugModes.Advanced;
	public PassModeType PassMode = PassModeType.Auto;
	[Range (0f, 0.1f)]
	public float
	updateThreshold = 0.01f;
	public Light _light;
	public bool useAmbientColor = true;
	public bool cameraBackgroundAsAmbientColor = true;
	[Range (0f, 2f)]
	public float
	ambientIntensity = 0.75f;
	public Material particlesMaterial;
	public int TextureColCount = 8;
	public int TextureRowCount = 8;
	public int MaxRenderDistance = 1000;
	public int LODstartDistance = 0;
	public bool AutoLOD = false;
	[Range (0f, 4f)]
	public float
	ScreensizeClipping = 1;
	[Range (0f, 1f)]
	public float
	ScreensizeClippingFade = 0.2f;

	public List<Puffy_Emitter> Emitters;

	private List<bool> EnabledEmitters = new List<bool>();

	public bool useThread = true;
	private List<SortIndex> mergedList = new List<SortIndex> ();
	private int live;
	private int _coresCount;
	private int _virtualCoresCount;
	private int activeGroups;
	private int TextureFrameCount = 1;
	private int meshUpdateCount;
	private int visibleMeshesCount;
	private int renderStep;
	private int visibleParticles;
	private float pDirectionalTextureFrameWidth = 1f;
	private double updateMeshTime;
	private double updateMeshTime_Step1;
	private double updateMeshTime_Step2;
	private double buildTime;
	private double renderTime;
	private double mergeTime;
	private DebugTimer frustumTime = new DebugTimer ();
	private DebugTimer sortTime = new DebugTimer ();
	private int createdGroupsCount;
	private SortGroup[] _sortGroups = new SortGroup[100];
	//private SortGroup[] _sortIndexStock = new SortGroup[100];

	private List<VariableMesh> meshList = new List<VariableMesh> ();
	private AnimationCurve[] luminosityCurveArray = new AnimationCurve[100];
	private OrderComparer comp = new OrderComparer ();
	private Vector2 _UVrotationVector = Vector2.zero;
	private Vector3[] _billboardShape = new Vector3[4];
	private Vector4[] _billboardTangents = new Vector4[4];
	private Camera _camera;
	private Transform _cameraTransform;
	private Transform _lightTransform;
	private bool allMeshCleared;
	private int renderPasses;

	private float materialElapsedTime = 0f;

	public static Puffy_Renderer GetCloudRenderer ()
	{
		for (int i = 0; i < instances.Count; i++)
			if (instances [i].particlesMaterial.name.ToLower ().Contains ("cloud"))
				return instances [i];
		return null;
	}

	public static Puffy_Renderer GetRenderer ()
	{
		if (instances.Count == 0)
			return null;
		return instances [0];
	}

	public static Puffy_Renderer GetRenderer (string rendererName)
	{
		for (int i = 0; i < instances.Count; i++)
			if (instances [i].name == rendererName)
				return instances [i];
		return null;
	}

	public static Puffy_Renderer GetRenderer (int index)
	{
		if (index < 0 || index >= instances.Count)
			return null;
		return instances [index];
	}

	private void OnDestroy ()
	{
		instances.Remove (this);
	}

	private void Awake ()
	{
		instances.Add (this);

		// number of real cores available in the cpu
		_coresCount = SystemInfo.processorCount;

		// virtual cores are used to split the sorting process when too many particles are created
		_virtualCoresCount = _coresCount;

		// create initial sort groups
		int i;
		createdGroupsCount = 0;
		for (i = 0; i < _coresCount; i++) {
			_sortGroups [i] = new SortGroup ();
			createdGroupsCount++;
		}

		if (!_light)
			_light = FindObjectOfType (typeof(Light)) as Light;

		// identify the main camera
		_camera = Camera.main;
		_cameraTransform = _camera.transform;

		if (_light != null) {
			_lightTransform = _light.transform;
	
			int layerIndex = LayerMask.NameToLayer ("PuffySmoke");
			if (layerIndex > -1) {
				_light.cullingMask &= ~(1 << layerIndex);
			}
		
		} else {
			_lightTransform = transform;
			Debug.LogWarning ("Warning ! No light has been found ! Using [" + name + "] gameobject transform instead !");
		}

		TextureFrameCount = (TextureColCount * TextureRowCount) - 1;

		pDirectionalTextureFrameWidth = 1f / TextureColCount;

		for(i=0;i<Emitters.Count;i++){
			EnabledEmitters.Add(Emitters[i].enabled);
		}

		// set the maximum particles count per mesh
		VariableMesh.maxParticlesCount = (int)meshSetup;
		
		double now = Time.realtimeSinceStartup;
		if (warmUp) {
			for (i = 0; i < warmUpCount; i++)
				AddMesh ();

			int count = (int)meshSetup * warmUpCount;
			for (i = 0; i < count; i++)
				mergedList.Add (null);
		}

		if (particlesMaterial)
			useTangentShader = particlesMaterial.HasProperty ("_Normal");

		if (debug)
			Debug.Log ("Renderer warm up took " + ((Time.realtimeSinceStartup - now) * 1000).ToString ("f3") + " ms");
	}

	private void OnGUI ()
	{
		if (debug) {
			double cpuTime = 0;

			foreach (Puffy_Emitter e in Emitters)
				cpuTime += (e.debugTime * 1000);

			cpuTime += frustumTime.Average ();
			cpuTime += sortTime.Average ();
			cpuTime += mergeTime;
			cpuTime += buildTime;
			cpuTime += updateMeshTime;

			if (debugMode == debugModes.Advanced) {
				float updateTime = 0f;
				live = 0;
				float total = 0;

				for(int i=0;i<Emitters.Count;i++) {
					if(Emitters[i].enabled){
						total += Emitters[i].particleTotal;
						live +=  Emitters[i].particleLive;
						updateTime += (float)( Emitters[i].debugTime * 1000);
					}
				}

				GUILayout.Space (30);

				GUILayout.Label ("CPU : " + cpuTime.ToString ("f2") + "ms");

				GUILayout.Label ("Live : " + live + "/" + total);

				if (debugString != "")
					GUILayout.Label ("debug " + debugString);

				if (live > 0) {
					GUILayout.Label ("Visible : " + visibleParticles);

					GUI.color = Color.white;
					if (useThread)
						GUILayout.Label ("Cores : " + _virtualCoresCount);
					else
						GUILayout.Label ("Threading OFF");


					GUILayout.Label ("Move : " + updateTime.ToString ("f2") + "ms");

					GUILayout.Label ("Frustum check : " + frustumTime.Average ().ToString ("f2") + "ms");
	
					GUILayout.Label ("Z Sort : " + sortTime.Average ().ToString ("f2") + "ms");
	
					GUILayout.Label ("Merge sort : " + (mergeTime * 1000).ToString ("f2") + "ms");

					GUILayout.Label ("Rebuild mesh : " + (buildTime * 1000).ToString ("f2") + "ms");

					GUILayout.Label ("Update mesh : " + (updateMeshTime * 1000).ToString ("f2") + "ms");
					if (useThread) {
						GUILayout.Label ("Update mesh A: " + (updateMeshTime_Step1 * 1000).ToString ("f2") + "ms");
						GUILayout.Label ("Update mesh B: " + (updateMeshTime_Step2 * 1000).ToString ("f2") + "ms");
					}

					GUILayout.Label ("Passes : " + renderPasses);


					GUILayout.Label ("TimeScale : " + Time.timeScale);


					string str = "sort groups " + activeGroups + "/" + createdGroupsCount + " :\n";
					for (int i = 0; i < createdGroupsCount; i++)
						if (_sortGroups [i] != null)
							str += "group " + i + " : " + _sortGroups [i].count + " " +
				(_sortGroups [i].sortTime > 0 ? (_sortGroups [i].sortTime * 1000).ToString ("f2") + "ms" : "") + "\n";

					GUILayout.Label (str);
				}
			} else {
				GUILayout.Space (30);
				GUILayout.Label ("CPU : " + cpuTime.ToString ("f2") + "ms");
			}
		}
	}

	private void OnDrawGizmos ()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere (transform.position, 0.3f);

		if (!_light) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine (transform.position, transform.position + transform.forward * 2);
		}

		if (debug) {
			int i = 0;

			var cols = new Color[16];

			cols [0] = new Color (1f, 0f, 0f, 1f);
			cols [1] = new Color (1f, 0.2f, 0f, 1f);
			cols [2] = new Color (1f, 0.4f, 0f, 1f);
			cols [3] = new Color (1f, 0.6f, 0f, 1f);
			cols [4] = new Color (1f, 0.8f, 0f, 1f);
			cols [5] = new Color (1f, 1f, 0f, 1f);
			cols [6] = new Color (0.8f, 1f, 0f, 1f);
			cols [7] = new Color (0.6f, 1f, 0f, 1f);
			cols [8] = new Color (0.4f, 1f, 0f, 1f);
			cols [9] = new Color (0.2f, 1f, 0f, 1f);
			cols [10] = new Color (0.0f, 1f, 0f, 1f);
			cols [11] = new Color (0.0f, 1f, 0.2f, 1f);
			cols [12] = new Color (0.0f, 1f, 0.4f, 1f);
			cols [13] = new Color (0.0f, 1f, 0.6f, 1f);
			cols [14] = new Color (0.0f, 1f, 0.8f, 1f);
			cols [15] = new Color (0.0f, 1f, 1f, 1f);

			for (i = 0; i < meshList.Count; i++)
				if (meshList [i].currentParticleCount > 0) {
					if (i <= 15)
						Gizmos.color = cols [i];
					Gizmos.DrawWireCube (
						meshList [i].mesh.bounds.center + meshList [i]._transform.position,
						meshList [i].mesh.bounds.size);
				}
		}
	}

	#region public methods

	public bool AddEmitter (Puffy_Emitter emitter)
	{
		if (emitter.puffyRenderer != this || Emitters.IndexOf (emitter) == -1) {
			if (emitter.puffyRenderer != this && emitter.puffyRenderer != null){
				emitter.puffyRenderer.RemoveEmitter (emitter);
			}

			Emitters.Add (emitter);
			EnabledEmitters.Add (emitter.enabled);

			return true;
		}

		return false;
	}

	public bool RemoveEmitter (Puffy_Emitter emitter)
	{
			
		if(Emitters.Remove(emitter)){
			EnabledEmitters.RemoveAt(0);
			for(int i=0;i<Emitters.Count;i++){
				EnabledEmitters[i] = Emitters[i].enabled;
			}
			return true;
		}
		return false;
	}

	public bool RemoveEmitter (int index)
	{
		if (index > -1 && index < Emitters.Count) {
			Emitters.RemoveAt (index);
			EnabledEmitters.RemoveAt(index);
			return true;
		}

		return false;
	}

	public bool RemoveEmitter (string emitterName)
	{
		for (int i = 0; i < Emitters.Count; i++)
			if (Emitters [i].name == emitterName) {
				Emitters.RemoveAt (i);
				EnabledEmitters.RemoveAt(i);
				return true;
			}

		return false;
	}

	#endregion

	private void LateUpdate ()
	{
		Render ();
	}

	private void Render ()
	{
		materialElapsedTime += Puffy_Emitter.globalFreeze?0:Time.deltaTime;
	
		if (renderStep == 0) {
			particlesPerCore_ChunkSize = (int)coresSetup;

			// total number of alive particles
			live = 0;

			// number of visible particles
			visibleParticles = 0;

			// get the total of alive particles
			for (int i = 0; i < Emitters.Count; i++)
				if (Emitters [i] == null) {
					Emitters.RemoveAt (i);
					EnabledEmitters.RemoveAt(i);
					i--;
				} else {

					// update the enabled state of the emitter
					EnabledEmitters[i] = Emitters [i].enabled && Emitters [i].gameObject.activeInHierarchy;

					if(Emitters [i].enabled){
						live += Emitters [i].particleLive;
					}
					if (Emitters [i].puffyRenderer == null){
						Emitters [i].puffyRenderer = this;
					}
				}
		}

		if (live > 0 || renderStep > 0 || visibleMeshesCount > 0)
		if (live == 0) {
			if (!allMeshCleared) {
				meshUpdateCount = 0;
				UpdateMeshes ();
			}
			renderStep = 0;
			materialElapsedTime = 0;
		} else {
			double time = 0;
			double totalTime = 0;

			if (renderStep == 0) {
				renderPasses = 0;
				time = Time.realtimeSinceStartup;

				// identify visible particles
				frustumTime.Start ();
				FrustumCheck ();
				frustumTime.Stop ();

				if (visibleParticles == 0) {
					if (!allMeshCleared) {
						meshUpdateCount = 0;
						UpdateMeshes ();
					}
					renderStep = 0;
				} else {
					// update the billboard shape
					updateBillboardsData ();

					// sort visible particles
					sortTime.Start ();
					SortParticles ();
					sortTime.Stop ();

					totalTime += Time.realtimeSinceStartup - time;

					if ((totalTime < updateThreshold || PassMode == PassModeType.One) && PassMode != PassModeType.Multiple)
						renderStep = 1;
				}
			}

			if (renderStep == 1) {
				// merge sorted groups
				time = Time.realtimeSinceStartup;
				MergeGroups ();
				mergeTime = Time.realtimeSinceStartup - time;
				totalTime += mergeTime;

				// rebuild all meshes data
				time = Time.realtimeSinceStartup;

				BuildMeshesThreads ();

				renderTime = Time.realtimeSinceStartup - time;
				totalTime += renderTime;

				if ((totalTime < updateThreshold || PassMode == PassModeType.One) && PassMode != PassModeType.Multiple)
					renderStep = 2;
			}

			if (renderStep == 2) {
				// send updated data to the meshes
				UpdateMeshes ();
				renderStep = 0;
			} else
				renderStep++;

			renderPasses++;
		}
	}

	private void updateBillboardsData ()
	{
		// light direction in camera space
		Vector3 lDir = _cameraTransform.InverseTransformDirection (_lightTransform.forward * -1);

		// particles Roll
		if (Mathf.Approximately (1f, lDir.z)) {
			lDir.x = 0f;
			lDir.y = 1f;
		} else {
			lDir.z = 0f;
			lDir.Normalize ();
		}

		// billboard shape
		float lAng = Mathf.Atan2 (lDir.y, lDir.x) + 0.7853982f; // + 3.141592f * 0.25f;
		lDir.x = Mathf.Cos (lAng) * 0.5f;
		lDir.y = Mathf.Sin (lAng) * 0.5f;

		// details roll
		_UVrotationVector.x = lDir.x;
		_UVrotationVector.y = lDir.y;

		if (useTangentShader) {
			Vector3 up = _cameraTransform.up;
			Vector3 right = _cameraTransform.right;

			_billboardShape [0] = (up + right) * -0.5f; // bottom left
			_billboardShape [1] = up * -0.5f + right * 0.5f; // bottom right
			_billboardShape [2] = up * 0.5f + right * -0.5f; // top left
			_billboardShape [3] = (up + right) * 0.5f; // top right

			SolveBillboardTangents ();

		} else {
			_billboardShape [0] = _cameraTransform.TransformDirection (new Vector3 (lDir.y, -lDir.x, 0f) * -1); // bottom right
			_billboardShape [1] = _cameraTransform.TransformDirection (new Vector3 (lDir.x, lDir.y, 0f) * -1); // top right
			_billboardShape [2] = _cameraTransform.TransformDirection (new Vector3 (-lDir.x, -lDir.y, 0f) * -1); // bottom left
			_billboardShape [3] = _cameraTransform.TransformDirection (new Vector3 (-lDir.y, lDir.x, 0f) * -1); // top left
		}


	}

	private void SolveBillboardTangents ()
	{
		const int triangleCount = 6;
		const int vertexCount = 4;

		var triangles = new[] {0, 2, 3, 3, 1, 0};

		var uvs = new Vector2[vertexCount] {
						new Vector2 (0, 0),
						new Vector2 (1, 0),
						new Vector2 (0, 1),
						new Vector2 (1, 1)
				};

		var tan1 = new Vector3[vertexCount];
		var tan2 = new Vector3[vertexCount];

		for (int a = 0; a < triangleCount; a += 3) {
			int i1 = triangles [a + 0];
			int i2 = triangles [a + 1];
			int i3 = triangles [a + 2];

			Vector3 v1 = _billboardShape [i1];
			Vector3 v2 = _billboardShape [i2];
			Vector3 v3 = _billboardShape [i3];

			Vector2 w1 = uvs [i1];
			Vector2 w2 = uvs [i2];
			Vector2 w3 = uvs [i3];
			//Debug.Log (w1+" "+w2+" "+w3);
			float x1 = v2.x - v1.x;
			float x2 = v3.x - v1.x;
			float y1 = v2.y - v1.y;
			float y2 = v3.y - v1.y;
			float z1 = v2.z - v1.z;
			float z2 = v3.z - v1.z;

			float s1 = w2.x - w1.x;
			float s2 = w3.x - w1.x;
			float t1 = w2.y - w1.y;
			float t2 = w3.y - w1.y;

			float div = s1 * t2 - s2 * t1;
			float r = div == 0.0f ? 0.0f : 1.0f / div;

			Vector3 sdir = new Vector3 ((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
			Vector3 tdir = new Vector3 ((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

			tan1 [i1] += sdir;
			tan1 [i2] += sdir;
			tan1 [i3] += sdir;

			tan2 [i1] += tdir;
			tan2 [i2] += tdir;
			tan2 [i3] += tdir;
		}

		billboardsNormal = _cameraTransform.forward * -1;

		Vector3 n = billboardsNormal;
		for (int a = 0; a < vertexCount; ++a) {
			Vector3 t = tan1 [a];
			Vector3 tmp = (t - n * Vector3.Dot (n, t)).normalized;
			_billboardTangents [a] = new Vector4 (
				tmp.x,
				tmp.y,
				tmp.z,
				(Vector3.Dot (Vector3.Cross (n, t), tan2 [a]) < 0.0f) ? -1.0f : 1.0f);
		}
	}

	// ***********************************************************************************************************************
	// STEP 1) Fill the sort groups with the visible particles indices
	// ***********************************************************************************************************************

	private ManualResetEvent[] doneEvents;
	private ManualResetEvent[] doneEvents2;
	private ManualResetEvent[] doneEvents3;
	private Matrix4x4 _lightMatrix;
	private Matrix4x4 _cameraMatrix;
	private Vector3 _cameraDirection;
	private Vector3 _cameraPosition;
	private float _cameraFOV;
	private float _cameraFOVangle;

	private void FrustumCheck ()
	{
		_lightMatrix = _lightTransform.worldToLocalMatrix;
		_cameraMatrix = _cameraTransform.worldToLocalMatrix;
		_cameraDirection = _cameraTransform.forward;
		_cameraPosition = _cameraTransform.position;
		_cameraFOV = _camera.fieldOfView;
		_cameraFOVangle = Mathf.Cos (_cameraFOV * Mathf.Deg2Rad * 0.9f);

		int i;

		// init the sort groups
		for (i = 0; i < createdGroupsCount; i++) {
			_sortGroups [i].index = 0;
			_sortGroups [i].count = 0;
		}

		// update the number of virtual cores according to the number of particles
		_virtualCoresCount = Mathf.Max (_coresCount, Mathf.CeilToInt (live / particlesPerCore_ChunkSize) + 1);
	
		if (doneEvents == null) {
			doneEvents = new ManualResetEvent[_virtualCoresCount];
			for (i=0; i<_virtualCoresCount; i++) {
				doneEvents [i] = new ManualResetEvent (false);
			}
		} else if (doneEvents.Length != _virtualCoresCount) {
			doneEvents = new ManualResetEvent[_virtualCoresCount];
			for (i=0; i<_virtualCoresCount; i++) {
				doneEvents [i] = new ManualResetEvent (false);
			}
		}

		//Action[] actions = new Action[_virtualCoresCount];
		// too few particles or only one core is available, do the computing on a single thread
		if (_coresCount == 1 || live < _coresCount * 512 || !useThread)
			for (i = 0; i < _virtualCoresCount; i++) {
				if (i >= createdGroupsCount) {
					_sortGroups [i] = new SortGroup ();
					createdGroupsCount++;
				}
				_sortGroups [i].index = 0;
				_sortGroups [i].count = 0;

				FrustumTask (i);
			}
		else {

			int threadCount = 0;

			// split the computing accross all virtual cores
			for (i = 0; i < _virtualCoresCount; i++) {
				if (i >= createdGroupsCount) {
					_sortGroups [i] = new SortGroup ();
					createdGroupsCount++;
				}

				_sortGroups [i].index = 0;
				_sortGroups [i].count = 0;
				
	
				doneEvents [i].Reset ();
				System.Threading.ThreadPool.QueueUserWorkItem (new System.Threading.WaitCallback (this.Threaded_FrustumTask), i);
	
				threadCount ++;
			}
			WaitHandle.WaitAll (doneEvents);
			
		}

		// total of visible particles
		visibleParticles = 0;
		activeGroups = 0;
		maxDistance = 0;
		minDistance = 0;
		SortGroup.distanceFactor = 1f;
		for (i = 0; i < createdGroupsCount; i++) {
			_sortGroups [i].index = 0;
			if (_sortGroups [i].count > 0){
				activeGroups++;
				maxDistance = Mathf.Max(maxDistance,_sortGroups [i].maxDistance);
				minDistance = Mathf.Min(minDistance,_sortGroups [i].minDistance);
			}
			visibleParticles += _sortGroups [i].count;
		}

		if(maxDistance > minDistance){
			SortGroup.minimalDistance = minDistance;
			float range = (maxDistance - minDistance);
			SortGroup.distanceFactor = 65535 / range;
			//Debug.Log ("min="+SortGroup.minimalDistance.ToString("f3")+" range="+range+" factor="+SortGroup.distanceFactor.ToString("f3"));
		}


	}

	private void Threaded_FrustumTask (System.Object stateInfo)
	{
		int index = (int)stateInfo;
		FrustumTask (index);
		doneEvents [index].Set ();
	}

	private float maxDistance;
	private float minDistance;
	private float distanceFactor;

	private void FrustumTask (int groupIndex)
	{
		int count = _sortGroups [groupIndex].index;

		//split = Mathf.Min (Mathf.Max (1, split), _virtualCoresCount);
		int split = Mathf.Min (Mathf.Max (1, _virtualCoresCount), _virtualCoresCount);

		float rowHeight = 1f / TextureRowCount;

		float excludeDistance4 = (MaxRenderDistance - LODstartDistance) / 4;
		float excludeDistance3 = (MaxRenderDistance - LODstartDistance) / 3;
		float excludeDistance2 = (MaxRenderDistance - LODstartDistance) / 2;

		Vector3 viewPosition;

		// used for radix sort to limit the distance range on 2 bytes
		//float distanceFactor = 65535 / MaxRenderDistance;

		float minDistance = float.MaxValue;
		float maxDistance = 0;

		// loop on all emitters
		for (int emitterIndex = 0; emitterIndex < Emitters.Count; emitterIndex++) {
			Puffy_Emitter emitter = Emitters [emitterIndex];

			if(EnabledEmitters[emitterIndex]){
				// define the chunk of particles to process for this emitter on this thread
				int stp = Mathf.CeilToInt ((float)emitter.particleLive / split);
				int start = groupIndex * stp;
				int end = Mathf.Min (start + stp, emitter.particleLive);

				if (end > start && end > 0) { // loop on particles

					bool excluded;
					int pointer,index,tile_index;
					float distance,dist,size,tile_U,tile_V;
					Vector2[] uvs;
					Vector3 direction,screenPoint;
					Puffy_ParticleData particle;
					SortIndex s;

//					Debug.Log (count+") "+start+" -> "+end+" / "+_sortGroups [groupIndex].items.Length);

					for (pointer = start; pointer < end; pointer++) {
						index = emitter.particlesPointers [pointer];

						particle = emitter.particles [index];

						direction = (particle.position - _cameraPosition).normalized;

						// check if the particle is in the camera view cone
						if (Vector3.Dot (_cameraDirection, direction) > _cameraFOVangle) {
							viewPosition = _cameraMatrix.MultiplyPoint3x4 (particle.position);

							distance = viewPosition.z;


							if (distance > 0) {
								// remove particles based on distance
					
								excluded = distance > MaxRenderDistance;

								if (!excluded) {
									if (AutoLOD) {
										dist = distance - LODstartDistance;
										excluded = dist > excludeDistance4 && (index % 4) == 0 || dist > excludeDistance3 && (index % 3) == 0 || dist > excludeDistance2 && (index % 2) == 0;
									}
									// reset near clipping fading value
									particle.alphaMultiplier = 1;

									if (!excluded && particle.size > 0 && distance < particle.size){
										// compute particle screen size
										screenPoint = Vector3.forward * distance + Vector3.up * particle.size * 0.5f;
										size = screenPoint.magnitude;
										if (size > 0)
											size = ((Mathf.Acos (distance / size) * Mathf.Rad2Deg) / _cameraFOV) * 2;

										if (size > ScreensizeClipping - ScreensizeClippingFade) {
											// compute fading value according to screen size and near clipping limit
											particle.alphaMultiplier = Mathf.Clamp01 ((ScreensizeClipping - size) / ScreensizeClippingFade);

											excluded = particle.alphaMultiplier == 0;
										}
									}
								}



								// if the particle is not excluded, then compute its texture tile coordinates
								if (!excluded) {

									maxDistance = Mathf.Max(maxDistance,distance);
									minDistance = Mathf.Min(minDistance,distance);

									s = _sortGroups [groupIndex].items [count];

									s.particleIndex = index;
									s.emitterIndex = emitterIndex;
									//s.order = (ushort)distance;
									s.distance = distance;

//									Debug.Log (groupIndex+") "+index+" ! "+count+":"+distance);

									if (!useTangentShader) {

										tile_index = Mathf.FloorToInt (Mathf.Max (0f, (1f - ((-_lightMatrix.MultiplyVector (direction).z + 1f) * 0.5f))) * TextureFrameCount);

										tile_U = pDirectionalTextureFrameWidth * (tile_index % TextureColCount);
										tile_V = rowHeight * Mathf.FloorToInt ((float)tile_index / TextureColCount);

										uvs = s.uvs;

										// bottom left
										uvs [0].x = tile_U;
										uvs [0].y = tile_V;

										// bottom right
										uvs [1].x = tile_U + pDirectionalTextureFrameWidth;
										uvs [1].y = tile_V;

										// top left
										uvs [2].x = tile_U;
										uvs [2].y = tile_V + rowHeight;

										// top right
										uvs [3].x = tile_U + pDirectionalTextureFrameWidth;
										uvs [3].y = tile_V + rowHeight;
									}
									count++;
								}
							}
						}
					}
				}
			}
		}
		_sortGroups[groupIndex].maxDistance = maxDistance;
		_sortGroups[groupIndex].minDistance = minDistance;

		_sortGroups [groupIndex].count = count;
		_sortGroups [groupIndex].sortTime = 0;
	}

	// ***********************************************************************************************************************
	// STEP 2) Sort the visible particles
	// ***********************************************************************************************************************

	private void Threaded_SortParticlesTask (System.Object stateInfo)
	{
		int index = (int)stateInfo;
		if(radixSort){
			_sortGroups [index].Sort(true);
		}else{
			Array.Sort (_sortGroups [index].items, 0, _sortGroups [index].count, comp);
		}

		doneEvents [index].Set ();
	}

	private void SortParticles ()
	{
		int i;
		int count = createdGroupsCount;
		int threadAdded = 0;
		double tmp;

		if (_coresCount == 1 || live < _coresCount * 512 || !useThread){ // sort everything on one thread
			for (i = 0; i < count; i++){
				if (_sortGroups [i].count > 0) {
					tmp = Time.realtimeSinceStartup;
					
					if(radixSort){
						_sortGroups [i].Sort(true);
					}else{
						Array.Sort (_sortGroups [i].items, 0, _sortGroups [i].count, comp);
					}

					_sortGroups [i].sortTime = Time.realtimeSinceStartup - tmp;
				} else
					_sortGroups [i].sortTime = 0;
			}
		}else {
			tmp = Time.realtimeSinceStartup;


			// loop on all used sort groups
			tmp = Time.realtimeSinceStartup;
			for (i = 0; i < count; i++) {
				_sortGroups [i].sortTime = 0;

				if (_sortGroups [i].count > 0) {
					doneEvents [i].Reset ();
					System.Threading.ThreadPool.QueueUserWorkItem (new System.Threading.WaitCallback (this.Threaded_SortParticlesTask), i);
					threadAdded++;
				}
			}
	 
			WaitHandle.WaitAll (doneEvents);
	
			_sortGroups [0].sortTime = Time.realtimeSinceStartup - tmp;
		}
	}

	// ***********************************************************************************************************************


	private VariableMesh AddMesh ()
	{
		VariableMesh vm = new VariableMesh (
			meshList.Count,
			particlesMaterial,
			(int)meshSetup,
			subMeshCount,
			warmUp,
			useTangentShader);
		meshList.Add (vm);
		return vm;
	}

	private void MergeGroups ()
	{
		int g, i;

		for (i = 0; i < visibleParticles; i++) {
			int groupIndex = -1;
			float maxValue = -1f;

			// loop on all sortgroups
			for (g = 0; g < createdGroupsCount; g++) {
				int index = _sortGroups [g].index;
				if (index < _sortGroups [g].count) {
					// find the group with the next farthest particle
					//float o = _sortGroups [g].items [index].order;
					float o = _sortGroups [g].items [index].distance;
					if (o >= maxValue) {
						groupIndex = g;
						maxValue = o;
					}
				}
			}

			if (groupIndex > -1) {
				if (i >= mergedList.Count) {
					mergedList.Add (_sortGroups [groupIndex].items [_sortGroups [groupIndex].index]);
				}else{
					mergedList [i] = _sortGroups [groupIndex].items [_sortGroups [groupIndex].index];
				}
				_sortGroups [groupIndex].index++;
			}
		}
	}

	private struct BuildStep
	{
		public int index;
		public int meshIndex;
		public int start;
		public int end;
		public int meshInternalIndex;
	}

	private bool useDetails;
	private Vector3 billboardsNormal = Vector3.forward;

	private void UpdateMaterial (Material material)
	{

		// if in editor, update any change that may have been made on the original shader while the application is running
		if (Application.isEditor && particlesMaterial != null)
			material.CopyPropertiesFromMaterial (particlesMaterial);

		// if the shader doesn't use a details texture then don't compute and send uv1 coordinates
		useDetails = material.HasProperty ("_DetailTex");

		if (_light != null) {
			material.SetFloat ("_LightIntensity", _light.enabled ? _light.intensity * 2 : 0);
			material.SetColor ("_LightColor", _light.enabled ? _light.color : Color.black);
		} else {
			material.SetFloat ("_LightIntensity", 1);
			material.SetColor ("_LightColor", Color.white);
		}

		if (useAmbientColor)
		if (cameraBackgroundAsAmbientColor)
			material.SetColor ("_AmbientColor", _camera.backgroundColor * ambientIntensity);
		else
			material.SetColor ("_AmbientColor", RenderSettings.ambientLight * ambientIntensity);
		else
			material.SetColor ("_AmbientColor", Color.black);

		if (material.HasProperty ("_Normal")) {
			material.SetVector ("_Normal", _cameraTransform.forward);
			useTangentShader = true;
		} else
			useTangentShader = false;

		if(material.HasProperty("_ElapsedTime")){
			material.SetFloat ("_ElapsedTime", materialElapsedTime);
		}
	}

	private BuildStep[] buildStepsArray = new BuildStep[100];

	private void Threaded_BuildMeshesTask (System.Object stateInfo)
	{
		BuildStep data = (BuildStep)stateInfo;
		BuildMeshes_task (data.index, data.meshIndex, data.start, data.end, data.meshInternalIndex);
		doneEvents2 [data.index].Set ();
	}

	private void BuildMeshesThreads ()
	{

		buildTime = Time.realtimeSinceStartup;

		if (visibleParticles > 0 && render) {
			// update shader parameters
			bool customBounds = true;
			int i;

			if (particlesMaterial != null) {

				if (_light != null) {
					particlesMaterial.SetFloat ("_LightIntensity", _light.intensity * 2);
					particlesMaterial.SetColor ("_LightColor", _light.enabled ? _light.color : Color.black);
				} else {
					particlesMaterial.SetFloat ("_LightIntensity", 1);
					particlesMaterial.SetColor ("_LightColor", Color.white);
				}

				if (useAmbientColor){
					if (cameraBackgroundAsAmbientColor){
						particlesMaterial.SetColor ("_AmbientColor", _camera.backgroundColor * ambientIntensity);
					}else{
						particlesMaterial.SetColor ("_AmbientColor", RenderSettings.ambientLight * ambientIntensity);
					}
				}else{
					particlesMaterial.SetColor ("_AmbientColor", Color.black);
				}

				// if the shader doesn't use a details texture then don't compute and send uv1 coordinates
				int useDetailsCounter = 0;
				for(i=0;i<particlesMaterial.shaderKeywords.Length;i++){
					if(particlesMaterial.shaderKeywords[i] == "EXTRA_LIGHTS_ON" ){
						if (particlesMaterial.HasProperty ("_ExtraLightsIntensity")){
							customBounds = particlesMaterial.GetFloat ("_ExtraLightsIntensity") <= 0.01f;
						}
					}
					// if the shader doesn't use a details texture then don't compute and send uv1 coordinates
					if(particlesMaterial.shaderKeywords[i] == "START_DETAILS_ON" || particlesMaterial.shaderKeywords[i] == "END_DETAILS_ON" ){
						useDetailsCounter++;
					}
				}
				useDetails = useDetailsCounter > 0;


			} else{
				Debug.LogError ("No material has been assigned to the Puffy_Renderer : " + name);
			}

			Vector3 lCameraPos = _cameraTransform.position;
			Vector3 lCameraForward = _cameraTransform.forward;


			int meshCount = Mathf.CeilToInt ((float)visibleParticles / VariableMesh.maxParticlesCount);
			meshUpdateCount = meshCount;

			int buildStepCount = meshCount;
			if (buildStepCount < _coresCount && visibleParticles > 512)
				buildStepCount = _coresCount;

			var steps = buildStepsArray;

			if (buildStepCount > buildStepsArray.Length) {
				buildStepsArray = new BuildStep[buildStepCount];
				steps = buildStepsArray;
			}

			int meshIndex = 0;
			int stepParticlesCount = Mathf.Min (
				VariableMesh.maxParticlesCount,
				Mathf.CeilToInt ((float)visibleParticles / _coresCount));
			
			if (buildStepCount == 1 || buildStepCount > meshCount && VariableMesh.maxParticlesCount < visibleParticles)
				stepParticlesCount = VariableMesh.maxParticlesCount;

			int start = 0;
			int end = 0;
			int accum = 0;

			for (i = 0; i < meshCount; i++) {
				if (i >= meshList.Count)
					AddMesh ();
				meshList [i].currentFirstParticleIndex = -1;
			}

			int start2 = 0;


			for (i = 0; i < buildStepCount; i++) {
				end += stepParticlesCount;
				accum += stepParticlesCount;

				if (accum > VariableMesh.maxParticlesCount && i > 0) {
					meshIndex++;
					accum = stepParticlesCount;
					start2 = 0;
				}

				if (end > visibleParticles)
					end = visibleParticles;

				if (meshIndex < meshCount) {
					steps [i].index = i;
					steps [i].meshIndex = meshIndex;
					steps [i].start = start;
					steps [i].end = end;
					steps [i].meshInternalIndex = start2;

					if (meshList [meshIndex].currentFirstParticleIndex == -1) meshList [meshIndex].currentFirstParticleIndex = start;
					meshList [meshIndex].currentLastParticleIndex = end - 1;

					start = end;
					start2 += stepParticlesCount;
				} else {
					buildStepCount = i;
					break;
				}
			}

			// prepare each mesh to receive particles data
			for (i = 0; i < meshCount; i++) {
				meshList [i].currentParticleCount = (meshList [i].currentLastParticleIndex - meshList [i].currentFirstParticleIndex) + 1;
				meshList [i].Init (meshList [i].currentParticleCount, debug);

				// compute Z-Depth center for the custom bounding box
				meshList [i].customBounds = customBounds;
				if (customBounds) {
					start = meshList [i].currentFirstParticleIndex;
					end = meshList [i].currentLastParticleIndex;
					/*
					if(start >= 0 && start < mergedList.Count) meshList [i].boundMin = lCameraPos + lCameraForward * mergedList [start].order;
					if(end >= 0 && end < mergedList.Count) meshList [i].boundMax = lCameraPos + lCameraForward * mergedList [end].order;
					*/
					if(start >= 0 && start < mergedList.Count) meshList [i].boundMin = lCameraPos + lCameraForward * mergedList [start].distance;
					if(end >= 0 && end < mergedList.Count) meshList [i].boundMax = lCameraPos + lCameraForward * mergedList [end].distance;
				}
			}


			if (!useThread || _coresCount == 1)
				for (i = 0; i < buildStepCount; i++)
					BuildMeshes_task (i, steps [i].meshIndex, steps [i].start, steps [i].end, steps [i].meshInternalIndex);
			else {
		
				int threadAdded = 0;

				if (doneEvents2 == null) {
					doneEvents2 = new ManualResetEvent[buildStepCount];
					for (i=0; i<buildStepCount; i++) {
						doneEvents2 [i] = new ManualResetEvent (false);
					}
				} else if (doneEvents2.Length != buildStepCount) {
					doneEvents2 = new ManualResetEvent[buildStepCount];
					for (i=0; i<buildStepCount; i++) {
						doneEvents2 [i] = new ManualResetEvent (false);
					}
				}


				for (i = 0; i < buildStepCount; i++) {
					doneEvents2 [i].Reset ();
					System.Threading.ThreadPool.QueueUserWorkItem (new System.Threading.WaitCallback (this.Threaded_BuildMeshesTask), steps [i]);
					threadAdded++;
				}
				if (threadAdded > 0) {
					WaitHandle.WaitAll (doneEvents2);
				}

			}
		}

		buildTime = Time.realtimeSinceStartup - buildTime;
	}

	private void BuildMeshes_task (
		int buildGroup,
		int meshIndex,
		int startIndex,
		int endIndex,
		int meshInternalParticleIndex)
	{
		SortIndex particlesData;

		Puffy_ParticleData p;

		VariableMesh currentMesh = meshList [meshIndex];

		VariableMeshData currentMeshData = currentMesh.currentMeshData;

		float size;
	


		float bs0x = _billboardShape [0].x;
		float bs0y = _billboardShape [0].y;
		float bs0z = _billboardShape [0].z;

		float bs1x = _billboardShape [1].x;
		float bs1y = _billboardShape [1].y;
		float bs1z = _billboardShape [1].z;

		float bs2x = _billboardShape [2].x;
		float bs2y = _billboardShape [2].y;
		float bs2z = _billboardShape [2].z;

		float bs3x = _billboardShape [3].x;
		float bs3y = _billboardShape [3].y;
		float bs3z = _billboardShape [3].z;

		byte colorR = 0;
		byte colorG = 0;
		byte colorB = 0;
		byte colorA = 0;

		float ageRatio;

		float uvOffsetX = 0;
		float uvOffsetY = 0;

		float _UVrotationVectorX = _UVrotationVector.x;
		float _UVrotationVectorY = _UVrotationVector.y;

		float posX = 0f;
		float posY = 0f;
		float posZ = 0f;

		float lCurveEvaluateValue = 0f;

		int i;
		int lv0, lv1, lv2, lv3;

		int firstVertex = 0;

		var lVerts = currentMeshData.vertices;

		Vector2[] lUVs = null;
		if (!useTangentShader)
			lUVs = currentMeshData.uvs;

		Vector2[] lUVs1 = null;
		if (useDetails)
			lUVs1 = currentMeshData.uvs1;

		var lExtra = currentMeshData.extraData;

		Vector4[] lTangents = null;
		if (useTangentShader)
			lTangents = currentMeshData.tangents;

		var lColors = currentMeshData.colors;

		Puffy_Emitter emitter = null;
		Color particleColor;

		float lLuminosityValue = 0f;

		AnimationCurve lLuminosityCurve;
		if (luminosityCurveArray [buildGroup] == null)
			luminosityCurveArray [buildGroup] = new AnimationCurve ();
		lLuminosityCurve = luminosityCurveArray [buildGroup];

		int prevEmitterIndex = -1;
		float lifeTime = 0;
		Puffy_Gradient particleGradient;

		for (i = startIndex; i < endIndex; i++) {
			particlesData = mergedList [i];

			if (prevEmitterIndex != particlesData.emitterIndex) {
				prevEmitterIndex = particlesData.emitterIndex;
				emitter = Emitters [prevEmitterIndex];

				if (emitter.useLuminosity)
					lLuminosityCurve.keys = emitter.luminosityCurve.keys;
			}

			p = emitter.particles [particlesData.particleIndex];

			posX = p.position.x;
			posY = p.position.y;
			posZ = p.position.z;

			size = p.size;

			ageRatio = p.ageRatio;
			lifeTime = p.lifetime;
			firstVertex = meshInternalParticleIndex * 4;

			lv0 = firstVertex;
			lv1 = firstVertex + 1;
			lv2 = firstVertex + 2;
			lv3 = firstVertex + 3;

			// extra data
			if (p.startLifetime > 0) {
				lExtra [lv0].x = ageRatio;
				lExtra [lv1].x = ageRatio;
				lExtra [lv2].x = ageRatio;
				lExtra [lv3].x = ageRatio;

			} else {
				// immortal particle for clouds
				lExtra [lv0].x = lifeTime;
				lExtra [lv1].x = lifeTime;
				lExtra [lv2].x = lifeTime;
				lExtra [lv3].x = lifeTime;
			}

			particleGradient = p.colorGradient;

			bool t1 = !Equals (particleGradient, null);

			if (t1 && !emitter.debugIntermediate) {
				particleColor = particleGradient.Evaluate (p.lifetime);

				colorR = (byte)(particleColor.r * 255);
				colorG = (byte)(particleColor.g * 255);
				colorB = (byte)(particleColor.b * 255);
				colorA = (byte)(particleColor.a * p.alphaMultiplier * 255);

				if (emitter.useLuminosity) {
					lCurveEvaluateValue = Mathf.Min (1f, p.lifetime / emitter.luminosityEndTime);
					lLuminosityValue = lLuminosityCurve.Evaluate (lCurveEvaluateValue);
				} else
					lLuminosityValue = 0;

				lExtra [lv0].y = lLuminosityValue;
				lExtra [lv1].y = lLuminosityValue;
				lExtra [lv2].y = lLuminosityValue;
				lExtra [lv3].y = lLuminosityValue;
			} else {
				if (emitter.debugIntermediate) {
					colorR = (byte)(p.startColor.r * 255);
					colorG = (byte)(p.startColor.g * 255);
					colorB = (byte)(p.startColor.b * 255);
					colorA = (byte)(p.startColor.a * p.alphaMultiplier * 255);
				} else {
					colorR = (byte)(p.color.r * 255);
					colorG = (byte)(p.color.g * 255);
					colorB = (byte)(p.color.b * 255);
					colorA = (byte)(p.color.a * p.alphaMultiplier * 255);
				}

				if (emitter.useLuminosity) {
					lCurveEvaluateValue = Mathf.Min (1f, p.lifetime / emitter.luminosityEndTime);
					lLuminosityValue = lLuminosityCurve.Evaluate (lCurveEvaluateValue);
				} else
					lLuminosityValue = 0;

				lExtra [lv0].y = lLuminosityValue;
				lExtra [lv1].y = lLuminosityValue;
				lExtra [lv2].y = lLuminosityValue;
				lExtra [lv3].y = lLuminosityValue;
			}

			lExtra [lv0].z = size;
			lExtra [lv1].z = size;
			lExtra [lv2].z = size;
			lExtra [lv3].z = size;

			// mesh center accumulation
			currentMesh.center.x += posX;
			currentMesh.center.y += posY;
			currentMesh.center.z += posZ;

			// position
			lVerts [lv0].x = posX + bs0x * size;
			lVerts [lv0].y = posY + bs0y * size;
			lVerts [lv0].z = posZ + bs0z * size;

			lVerts [lv1].x = posX + bs1x * size;
			lVerts [lv1].y = posY + bs1y * size;
			lVerts [lv1].z = posZ + bs1z * size;

			lVerts [lv2].x = posX + bs2x * size;
			lVerts [lv2].y = posY + bs2y * size;
			lVerts [lv2].z = posZ + bs2z * size;

			lVerts [lv3].x = posX + bs3x * size;
			lVerts [lv3].y = posY + bs3y * size;
			lVerts [lv3].z = posZ + bs3z * size;

			// color
			lColors [lv0].r = colorR;
			lColors [lv0].g = colorG;
			lColors [lv0].b = colorB;
			lColors [lv0].a = colorA;

			lColors [lv1].r = colorR;
			lColors [lv1].g = colorG;
			lColors [lv1].b = colorB;
			lColors [lv1].a = colorA;

			lColors [lv2].r = colorR;
			lColors [lv2].g = colorG;
			lColors [lv2].b = colorB;
			lColors [lv2].a = colorA;

			lColors [lv3].r = colorR;
			lColors [lv3].g = colorG;
			lColors [lv3].b = colorB;
			lColors [lv3].a = colorA;

			// uv tile
			if (!useTangentShader) {
				lUVs [lv0].x = particlesData.uvs [0].x;
				lUVs [lv0].y = particlesData.uvs [0].y;

				lUVs [lv1].x = particlesData.uvs [1].x;
				lUVs [lv1].y = particlesData.uvs [1].y;

				lUVs [lv2].x = particlesData.uvs [2].x;
				lUVs [lv2].y = particlesData.uvs [2].y;

				lUVs [lv3].x = particlesData.uvs [3].x;
				lUVs [lv3].y = particlesData.uvs [3].y;
			} else {
				lTangents [lv0] = _billboardTangents [0];
				lTangents [lv1] = _billboardTangents [1];
				lTangents [lv2] = _billboardTangents [2];
				lTangents [lv3] = _billboardTangents [3];
			}

			// uv smoke details
			if (useDetails) {

				uvOffsetX = uvOffsetY = p.randomSeed; // offset anim

				lUVs1 [lv0].x = uvOffsetX - _UVrotationVectorX;
				lUVs1 [lv0].y = uvOffsetY - _UVrotationVectorY;

				lUVs1 [lv1].x = uvOffsetX + _UVrotationVectorY;
				lUVs1 [lv1].y = uvOffsetY - _UVrotationVectorX;

				lUVs1 [lv2].x = uvOffsetX - _UVrotationVectorY;
				lUVs1 [lv2].y = uvOffsetY + _UVrotationVectorX;

				lUVs1 [lv3].x = uvOffsetX + _UVrotationVectorX;
				lUVs1 [lv3].y = uvOffsetY + _UVrotationVectorY;
			}

			meshInternalParticleIndex++;
		}

		lVerts = null;
		lUVs = null;
		lUVs1 = null;
		lColors = null;
	}
		
	private void Threaded_UpdateMeshesTask (System.Object stateInfo)
	{
		int index = (int)stateInfo;
		meshList [index].UpdateMesh_Step1 ();
		doneEvents3 [index].Set ();
	}

	private void UpdateMeshes ()
	{
		updateMeshTime = Time.realtimeSinceStartup;
		visibleMeshesCount = 0;
		updateMeshTime_Step1 = 0;
		updateMeshTime_Step2 = 0;

		if (meshList.Count > 0) {
			int i;
			if (useThread) {
				// multi threads update
				updateMeshTime_Step1 = Time.realtimeSinceStartup;

				int threadAdded = 0;

				if (doneEvents3 == null) {
					doneEvents3 = new ManualResetEvent[meshUpdateCount];
					for (i=0; i<meshUpdateCount; i++) {
						doneEvents3 [i] = new ManualResetEvent (false);
					}
				} else if (doneEvents3.Length != meshUpdateCount) {
					doneEvents3 = new ManualResetEvent[meshUpdateCount];
					for (i=0; i<meshUpdateCount; i++) {
						doneEvents3 [i] = new ManualResetEvent (false);
					}
				}
		
									
				for (i = 0; i < meshUpdateCount; i++) {
					int _i = i;
					doneEvents3 [i].Reset ();
						
					System.Threading.ThreadPool.QueueUserWorkItem (new System.Threading.WaitCallback (this.Threaded_UpdateMeshesTask), _i);
															
					threadAdded++;
					visibleMeshesCount ++;
				}
				WaitHandle.WaitAll (doneEvents3);
		
				threadAdded = 0;
								
				updateMeshTime_Step1 = Time.realtimeSinceStartup - updateMeshTime_Step1;
			} else {
				// update on one core
				updateMeshTime_Step1 = Time.realtimeSinceStartup;
				for (i = 0; i < meshUpdateCount; i++) {
					meshList [i].UpdateMesh_Step1 ();
					visibleMeshesCount ++;
				}
				updateMeshTime_Step1 = Time.realtimeSinceStartup - updateMeshTime_Step1;
			}

			// send updated data to gpu
			updateMeshTime_Step2 = Time.realtimeSinceStartup;
			for (i = 0; i < meshUpdateCount; i++) {
				meshList [i].UpdateMesh_Step2 (!useTangentShader, useDetails);
				UpdateMaterial (meshList [i].material);
			}
			updateMeshTime_Step2 = Time.realtimeSinceStartup - updateMeshTime_Step2;

			// clear unused meshes
			for (i = meshUpdateCount; i < meshList.Count; i++)
				meshList [i].ClearMesh ();
		}
		updateMeshTime = Time.realtimeSinceStartup - updateMeshTime;
		allMeshCleared = meshUpdateCount == 0;
	}

	private class SortGroup
	{
		public static float minimalDistance;
		public static float distanceFactor;
		public int index;
		public int count;
		public double sortTime;
		public SortIndex[] items;

		public float maxDistance;
		public float minDistance;

		private bool initDone = false;

		public SortGroup ()
		{
			items = new SortIndex[particlesPerCore_ChunkSize];

			for (int i = 0; i < particlesPerCore_ChunkSize; i++){
				items[i] = new SortIndex ();
			}

		}

		/* RADIX SORT */

		private KVEntry[] digits = new KVEntry[1];
		private KVEntry[] SortedDigits = new KVEntry[1];
		private int itemCount = 0;
		private int[] ArrayB = new int[1];

		public void Sort(bool reverse = false){
			if(!initDone) Init(particlesPerCore_ChunkSize);
			itemCount = count;
			items = RadixSortAux(items,0,reverse);
		}

		void Init(int cnt){
			initDone = true;
			if(digits.Length < cnt){
				digits = new KVEntry[cnt];
				SortedDigits = new KVEntry[cnt];
				for(int i=0;i<cnt;i++){
					digits[i] = new KVEntry();
					SortedDigits[i] = new KVEntry();
				}
			}
		}

		private SortIndex[] RadixSortAux(SortIndex[] array, int digit=0, bool reverse = false)
		{
			SortIndex[] SortedArray = new SortIndex[items.Length];

			int max = (array[0].order>>(digit<<3)) & 0xFF;
			for (int i = 0; i < itemCount; i++)
			{
				digits[i].Key = i;
				if(digit==0){
					array[i].order = (ushort)((array[i].distance - minimalDistance) * distanceFactor);
				}
				digits[i].Value = (array[i].order>>(digit<<3)) & 0xFF;
				if(digits[i].Value > max) max = digits[i].Value;
			}
			
			if (digit > 1){
//				Debug.Log ("------------------------");
				if(reverse){
					int j = itemCount;
					for(int i=0;i<itemCount;i++){
						j--;
						items[i] = array[j];
//						Debug.Log (items[i].order+" -> "+(items[i].distance-minimalDistance));
					}
				}else{
					for(int i=0;i<itemCount;i++){
						items[i] = array[i];
					}
				}
				return items;
			}

			CountingSort(digits,max);
			
			for (int i = 0; i < itemCount; i++){
				SortedArray[i] = array[SortedDigits[i].Key];
			}

			return RadixSortAux(SortedArray, digit+1,reverse);
		}

		private void CountingSort(KVEntry[] ArrayA,int maxValue)
		{
			int subcount = maxValue + 1;
			if(subcount > ArrayB.Length){
				ArrayB = new int[subcount];
			}
			
			for (int i = 0; i < subcount; i++)
				ArrayB[i] = 0;
			
			for (int i = 0; i < itemCount; i++)
				ArrayB[ArrayA[i].Value]++;
			
			for (int i = 1; i < subcount; i++)
				ArrayB[i] += ArrayB[i - 1];
			
			for (int i = itemCount - 1; i >= 0; i--)
			{
				int value = ArrayA[i].Value;
				int idx = ArrayB[value];
				ArrayB[value]--;
				SortedDigits[idx - 1].Key = i;
				SortedDigits[idx - 1].Value = value;
			}
		}

		struct KVEntry
		{
			public int Key;
			public int Value;
		}
	}

	private class SortIndex
	{
		public int emitterIndex;
		public int particleIndex;
		public Vector2[] uvs = new Vector2[4];
		public ushort order;
		public float distance;
	}

	private class OrderComparer : IComparer<SortIndex>
	{
		public int Compare (SortIndex a, SortIndex b)
		{
			// sort back to front
			return (a.distance > b.distance) ? -1 : 1;
		}
	}

	private class VariableMesh
	{
		public static int maxParticlesCount = 1024;
		private List<VariableMeshData> meshData = new List<VariableMeshData> ();
		public Vector3 boundMin = Vector3.zero;
		public Vector3 boundMax = Vector3.zero;
		public GameObject gameObject;
		public Mesh mesh;
		public Vector3 center = Vector3.zero;
		public Material material;
		public int meshDataIndex;
		public int particleIndex;
		public int currentParticleCount;
		public double updateTime;
		public Transform _transform;
		private bool needUpdate = true;
		private MeshRenderer meshRenderer;
		public int currentFirstParticleIndex = -1;
		public int currentLastParticleIndex = -1;
		private int currentTriangleCount;
		public VariableMeshData currentMeshData;
		private string name = "";
		public bool customBounds = true;
		private bool debug = false;
		private int prevParticleCount; // used only in editor when debug is on

		public VariableMesh (
			int n = 0,
			Material mat = null,
			int maxCount = -1,
			int subMeshCount = 4,
			bool warmup = false,
			bool useTangentShader = false)
		{
			if (maxCount > 0)
				maxParticlesCount = maxCount;

			// create multiple mesh buffers with different vertex counts
			if (subMeshCount < 1)
				subMeshCount = 1;

			int step = Mathf.FloorToInt (maxParticlesCount / subMeshCount);
			int i = step;

			name = "Puffy_MESH_" + n + " ";

			while (i <= maxParticlesCount) {
				int count = Mathf.Min (i, maxParticlesCount);
				meshData.Add (new VariableMeshData (count, warmup, useTangentShader));
				if (debug)
					name += "/" + i;
				i += step;
			}

			mesh = new Mesh ();

			mesh.MarkDynamic ();

			gameObject = new GameObject ();

			int layerIndex = LayerMask.NameToLayer ("PuffySmoke");
			if (layerIndex > -1)
				gameObject.layer = layerIndex;

			gameObject.name = name;
			MeshFilter mf = gameObject.AddComponent <MeshFilter>() as MeshFilter;
			mf.mesh = mesh;

			meshRenderer = gameObject.AddComponent <MeshRenderer>() as MeshRenderer;
			meshRenderer.material = mat;
			meshRenderer.material.color = Color.white;
			material = meshRenderer.material;

			gameObject.GetComponent<Renderer>().receiveShadows = false;
			gameObject.GetComponent<Renderer>().castShadows = true;
			gameObject.SetActive (false);

			_transform = gameObject.transform;

			if (warmup) {
				UpdateMesh_Step1 ();
				UpdateMesh_Step2 (true);
			}
		}

		// get the meshdata index closest to the current particle count
		public int getMeshDataIndex (int particleCount)
		{
			meshDataIndex = 0;
			while (meshData[meshDataIndex].particleCount < particleCount) {
				meshDataIndex++;
				if (meshDataIndex >= meshData.Count) {
					meshDataIndex = meshData.Count - 1;
					break;
				}
			}
			return meshDataIndex;
		}

		public VariableMeshData getMeshData (int particleCount)
		{
			int i = getMeshDataIndex (particleCount);

			if (i < 0)
				return null;

			return meshData [i];
		}

		public VariableMeshData Init (int particleCount, bool debug = false)
		{
			particleIndex = 0;
			center = Vector3.zero;

			needUpdate = true;

			currentMeshData = getMeshData (particleCount);

#if UNITY_EDITOR
			if (debug && prevParticleCount != currentMeshData.particleCount)
			{
				prevParticleCount = currentMeshData.particleCount;
				gameObject.name = name + " : " + (prevParticleCount);
			}
#endif

			return currentMeshData;
		}

		public double ClearMesh ()
		{
			updateTime = 0;
			if (meshRenderer.enabled) {
				updateTime = Time.realtimeSinceStartup;
				meshRenderer.enabled = false;
				mesh.Clear(false);
				mesh.RecalculateBounds ();
				particleIndex = 0;
				currentParticleCount = 0;
				currentTriangleCount = 0;
				updateTime = Time.realtimeSinceStartup - updateTime;

#if UNITY_EDITOR
				if (gameObject.activeInHierarchy)
					gameObject.name = name;
#endif

				gameObject.SetActive (false);
			}
			return updateTime;
		}

		public void UpdateMesh_Step1 ()
		{
			if (needUpdate && currentParticleCount > 0) {
				if (center.sqrMagnitude > 0 && currentParticleCount > 0)
					center /= currentParticleCount;

				int i;
				int end = currentParticleCount * 4;
				int cnt = meshData [meshDataIndex].vertexCount;

				float posX = center.x;
				float posY = center.y;
				float posZ = center.z;

				// local variable for faster access
				var lVerts = meshData [meshDataIndex].vertices;

				// update all used vertices to reflect the center offset
				for (i = 0; i < end; i++) {
					lVerts [i].x -= posX;
					lVerts [i].y -= posY;
					lVerts [i].z -= posZ;
				}

				// unused vertices are moved to the position of the first vertex
				posX = lVerts [0].x;
				posY = lVerts [0].y;
				posZ = lVerts [0].z;

				for (i = end; i < cnt; i++) {
					lVerts [i].x = posX;
					lVerts [i].y = posY;
					lVerts [i].z = posZ;
				}
				lVerts = null;
			}
		}

		public void UpdateMesh_Step2 (bool sendUV0 = true, bool sendUV1 = true)
		{
			if (needUpdate && currentParticleCount > 0) {
				needUpdate = false;

				int newTriangleCount = meshData [meshDataIndex].trianglesCount;
				bool sameMesh = currentTriangleCount == newTriangleCount;

				if (!sameMesh)
					mesh.Clear (false);

				mesh.MarkDynamic ();
				mesh.vertices = meshData [meshDataIndex].vertices;

				if (sendUV0){
					// volumetric sprite map, uv0 are redefined each step
					mesh.uv = meshData [meshDataIndex].uvs;
				}else{
					// normal map : the tangents are updated each step and uv0 are not modified
					mesh.tangents = meshData [meshDataIndex].tangents;
				}

				if (sendUV1)
					mesh.uv2 = meshData [meshDataIndex].uvs1;

				mesh.colors32 = meshData [meshDataIndex].colors;
				mesh.normals = meshData [meshDataIndex].extraData;


				if (!sameMesh) {
					if (!sendUV0){
						// normal map mode : uv0 needs to be updated once
						mesh.uv = meshData [meshDataIndex].uvs;
					}

					mesh.triangles = meshData [meshDataIndex].triangles;

					currentTriangleCount = newTriangleCount;
				}

				// move the gameobject
				_transform.position = center;

				// custom faster bounds can be used when no extralights rendering is involved
				if (customBounds) // force custom bounds to fix the Z fighting between meshes
					mesh.bounds = new Bounds (
						(boundMax + boundMin) * 0.5f - center,
						Vector3.one * Mathf.Abs (boundMin.magnitude - boundMax.magnitude));
				else // for proper extra lights rendering, real bounds must be calculated
					mesh.RecalculateBounds ();

				meshRenderer.enabled = true;
				if (!gameObject.activeSelf)
					gameObject.SetActive (true);
			}
		}
	}

	private class VariableMeshData
	{
		public readonly int particleCount;
		public readonly int vertexCount;
		public readonly int[] triangles;
		public readonly Vector2[] uvs;
		public readonly Vector2[] uvs1;
		public readonly Vector3[] vertices;
		public readonly Vector3[] extraData;
		public readonly Color32[] colors;
		public readonly Vector4[] tangents;
		public readonly int trianglesCount;

		public VariableMeshData (int _particleCount, bool warmup = false, bool useTangentShader = false)
		{
			particleCount = _particleCount;

			vertexCount = particleCount * 4;

			vertices = new Vector3[vertexCount];
			uvs = new Vector2[vertexCount];
			uvs1 = new Vector2[vertexCount];
			colors = new Color32[vertexCount];
			extraData = new Vector3[vertexCount];
			if (useTangentShader)
				tangents = new Vector4[vertexCount];

			trianglesCount = particleCount * 6;

			triangles = new int[trianglesCount];

			int v;

			for (v = 0; v < particleCount; v++) {
				int j0 = v * 4;
				int k = v * 6;

				int j1 = j0 + 1;
				int j2 = j0 + 2;
				int j3 = j0 + 3;

				vertices [j0] = vertices [j1] = vertices [j2] = vertices [j3] = Vector3.zero;

				extraData [j0] = extraData [j1] = extraData [j2] = extraData [j3] = Vector3.zero;

				colors [j0] = colors [j1] = colors [j2] = colors [j3] = new Color32 (1, 1, 1, 1);

				uvs [j0] = Vector2.zero;
				uvs [j1] = new Vector2 (1, 0);
				uvs [j2] = new Vector2 (0, 1);
				uvs [j3] = new Vector2 (1, 1);

				if (useTangentShader)
					tangents [j0] = tangents [j1] = tangents [j2] = tangents [j3] = Vector4.zero;

				triangles [k] = j0;
				triangles [k + 1] = j2;
				triangles [k + 2] = j3;
				triangles [k + 3] = j3;
				triangles [k + 4] = j1;
				triangles [k + 5] = j0;

				uvs1 [j0] = new Vector2 (-0.5f, -0.5f); // bottom left
				uvs1 [j1] = new Vector2 (0.5f, -0.5f); // bottom right
				uvs1 [j2] = new Vector2 (-0.5f, 0.5f); // top left
				uvs1 [j3] = new Vector2 (0.5f, 0.5f); // top right
			}
			//Debug.Log ("Mesh data created with "+vertices.Length+" vertices and "+triangles.Length+" triangles for a maximum of "+particleCount+" particles");
		}
	}
}

public class DebugTimer
{
	private float[] times = new float[50];
	private double timestamp;
	public float elapsed;
	public float maximum;
	public float minimum;

	public void Start ()
	{
		timestamp = Time.realtimeSinceStartup;
	}

	public void Stop ()
	{
		timestamp = Time.realtimeSinceStartup - timestamp;
		elapsed = (float)(timestamp * 1000);
		for (int i = 0; i < 49; i++)
			times [i] = times [i + 1];
		times [49] = elapsed;
	}

	public float Average ()
	{
		float a = 0f;
		maximum = 0f;
		minimum = 99999f;
		for (int i = 0; i < 50; i++) {
			a += times [i];
			maximum = Mathf.Max (maximum, times [i]);
			minimum = Mathf.Min (minimum, times [i]);
		}

		return a / 50f;
	}

	public void ShowGraph (float offsetY = 0f)
	{
		float nearClip = Camera.main.nearClipPlane + 0.5f;

		GL.Begin (GL.LINES);

		Vector3 pos = Camera.main.ScreenToWorldPoint (new Vector3 (300, 100 + offsetY, nearClip));
		Vector3 up = Camera.main.transform.up;
		Vector3 right = Camera.main.transform.right;

		var colors = new Color[5];

		colors [0] = new Color (1f, 1f, 1f, 1f);
		colors [1] = new Color (0f, 1f, 0f, 1f);
		colors [2] = new Color (1f, 1f, 0f, 1f);
		colors [3] = new Color (1f, 0.5f, 0f, 1f);
		colors [4] = new Color (1f, 0f, 0f, 1f);

		GL.Color (new Color (1f, 1f, 1f, 0.5f));

		GL.Vertex (pos);
		GL.Vertex (pos + right * 0.25f);

		GL.Vertex (pos + up / 50f);
		GL.Vertex (pos + up / 50f + right * 0.25f);

		GL.Vertex (pos + (up * 2) / 50f);
		GL.Vertex (pos + (up * 2) / 50f + right * 0.25f);

		int c = 0;
		for (int i = 0; i < 50; i++) {
			c = Mathf.Min (4, Mathf.FloorToInt (times [i]));
			GL.Color (colors [c]);
			GL.Vertex (pos);
			GL.Vertex (pos + up * times [i] / 50f);
			pos += right * 0.005f;
		}
		GL.End ();
	}
}
