using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Puffy_PerlinCloud : MonoBehaviour {
	
	protected Transform _transform;
	private Vector3 _forward;
	private Vector3 _right;
	private Vector3 _position;
	private Vector3 _size;
	
	public Puffy_Emitter emitter;
	
	public int randomSeed = 0;
	public bool absolutePosition = false;
	
	private Vector3 perlinPosition = Vector3.zero;
	public Vector3 perlinPositionOffset = Vector3.zero;
	public Vector3 perlinArea = Vector3.one;
	
	[Range(1,4)]
	public int NoiseOctaves = 1;
	
	[Range(0.1f,20f)]
	public float NoiseFrequency = 1;
	
	[Range(0.1f,4f)]
	public float NoiseAmplitude = 1;
	
	
	[Range(0.1f,100f)]
	public float particlesScale = 1f;
	
	[Range(0,16000)]
	public int particlesCount = 10;
	
	[Range(0.1f,2f)]
	public float opacity = 1f;
	
	[Range(0.0f,2f)]
	public float gizmoThreshold = 0f;
	
	private float _opacity = 1f;
	
	private int _NoiseOctaves = 0;
	private float _NoiseFrequency = 0;
	private float _NoiseAmplitude = 0;
	
	private List<int> particlesList;
	private List<Vector3> particlesPositions;
	private List<float> particlesSizes;
	
	private int _randomSeed;
	private int _particlesCount;
	private Vector3 _perlinArea;

	private float _particlesScale;
	private Vector3 _perlinPosition = Vector3.zero;
	private Vector3 _perlinScale = Vector3.one;
	private Vector3 _perlinPositionOffset = Vector3.zero;
	
	private bool built = false;
	
	private PerlinNoise perlin;
	
	// Use this for initialization
	void Awake() {
		
	}
	
	void Start(){
		Init ();
	}
	
	void Init(){
		_transform = transform;
		if(!emitter) emitter = gameObject.GetComponent<Puffy_Emitter>();
		emitter.cloudEmitter = true;
		particlesList = new List<int>();
		particlesPositions = new List<Vector3>();
		particlesSizes = new List<float>();

		perlin = new PerlinNoise(0);
	}
	
	// Use this for initialization
	void Build() {
		if(Application.isPlaying && !emitter) return;
		
		if(perlin == null) Init ();
		
		if(particlesCount<0) particlesCount = 0;
		
		_randomSeed = randomSeed;
		_particlesCount = particlesCount;

		_particlesScale = particlesScale;
		_perlinPosition = perlinPosition;	
		_perlinArea = perlinArea;
		_perlinPositionOffset = perlinPositionOffset;
		_NoiseOctaves = NoiseOctaves;
		_NoiseFrequency = NoiseFrequency;
		_NoiseAmplitude = NoiseAmplitude;
		_opacity = opacity;

		_perlinScale = _transform.localScale;

		int i = 0;
		Vector3 pos;
		float size;
		
		List<int> existingParticles = new List<int>(); 
		
		
		if(Application.isPlaying){
			for(i=0; i<particlesList.Count;i++){
				existingParticles.Add (particlesList[i]);
			}
		}else{
			existingParticles.Clear();
		}
		particlesList.Clear();
		particlesPositions.Clear();
		particlesSizes.Clear();
		_position += Vector3.one;
		
		int index = 0;
		i = 0;
		//int subtotal = 1;
		Vector3 perlinPos = Vector3.zero;
		int added = 0;
		float noiseValue;
		
		UnityEngine.Random.seed = randomSeed;
		perlin.Seed(randomSeed);
		//float step = 0.1f; //1f / Mathf.Pow(_particlesCount, 1f / 3f);
		
		Vector3 mv = perlinArea * -1;
	
		pos = mv;
		Color coul = new Color(1,1,1,1);
		float alpha = 0;
		while(added < _particlesCount && i < _particlesCount){

			pos = Random.insideUnitSphere;

			pos.x *= perlinArea.x;
			pos.y *= perlinArea.y;
			pos.z *= perlinArea.z;

			perlinPos = pos + perlinPosition + perlinPositionOffset;


			noiseValue = Mathf.Abs(perlin.FractalNoise(perlinPos,NoiseOctaves,NoiseFrequency,NoiseAmplitude));
			alpha = noiseValue * noiseValue * 2;

			if(alpha <= 0.1f && added > 1){
				index = Random.Range(0,added-1);
				index = added-1;
				pos = particlesPositions[index] + Random.insideUnitSphere * particlesSizes[index] * 2;
				perlinPos = pos + perlinPosition + perlinPositionOffset;

				noiseValue = Mathf.Abs(perlin.FractalNoise(perlinPos,NoiseOctaves,NoiseFrequency,NoiseAmplitude));
				alpha = noiseValue * noiseValue * 2;
			}

			if(alpha > 0.1f ){
				size = noiseValue*noiseValue*2;
				
				coul.r = emitter.startColor.r;
				coul.g = emitter.startColor.g;
				coul.b = emitter.startColor.b;
				coul.a = Mathf.Clamp01(emitter.startColor.a * alpha * opacity);
				
				if(Application.isPlaying){
					if(existingParticles.Count > 0){
						
						index = existingParticles[0];
						existingParticles.RemoveAt(0);
						
						emitter.particles[index].position = pos;
						emitter.particles[index].size = size * particlesScale;
						emitter.particles[index].startSize = size * particlesScale;
						emitter.particles[index].endSize = size * particlesScale;
						emitter.particles[index].startLifetime = -1;
						emitter.particles[index].lifetime = 0;
						emitter.particles[index].speed = 0f;
	
						emitter.particles[index].startColor = coul;
						emitter.particles[index].endColor = coul;
						emitter.particles[index].color = coul;
											
						emitter.particles[index].randomSeed = Random.Range(0f,0.5f);
						
					}else{
						index = emitter.SpawnParticle(pos,Vector3.zero,0,-1,size*particlesScale,size*particlesScale,coul,coul,0);
					}
					
				}else{
					index = added;	
				}
				
				if(index >= 0){
					particlesList.Add (index);
					particlesPositions.Add (pos);
					particlesSizes.Add (size);
					added++;
					i-=10;
				}
			}
			i++;
		}
		
		if(Application.isPlaying){
			for(i=0; i < existingParticles.Count;i++){
				emitter.KillParticle(existingParticles[i]);
			}
		}
		built = true;
	}
	
	
	void Update(){
		if(absolutePosition) perlinPosition = _transform.position;
		
		if(!built || _randomSeed!=randomSeed || _particlesCount!=particlesCount || _perlinArea!=perlinArea
		   || _perlinScale != _transform.localScale	
		   || _particlesScale != particlesScale || _perlinPosition != perlinPosition || perlinPositionOffset != _perlinPositionOffset
			|| _NoiseOctaves != NoiseOctaves || _NoiseFrequency != NoiseFrequency || _NoiseAmplitude != NoiseAmplitude
			|| _opacity != opacity
			) Build();
		
		if(_transform.position != _position || _transform.forward != _forward || _transform.right != _right || _transform.localScale != _size ){
			_position = _transform.position;
			_forward = _transform.forward;
			_right = _transform.right;
			_size = _transform.localScale;
			
			
			if(Application.isPlaying){
				int i,index;
				for(i = 0; i < particlesList.Count ; i++){
					index = particlesList[i];
					emitter.particles[index].position = _transform.TransformPoint(particlesPositions[i]);
					emitter.particles[index].size = particlesSizes[i] * particlesScale;
				}
			}
		}
	}
	
	void OnDestroy(){
		if(Application.isPlaying){
			int i,index;
			for(i = 0; i < particlesList.Count ; i++){
				index = particlesList[i];
				emitter.particles[index].dead = true;
			}
		}
	}
	
	#if UNITY_EDITOR
	void OnDrawGizmos(){
		if(!Application.isPlaying && particlesPositions!=null){
			_transform = transform;
			
			if(emitter != null){
				int i;
				Vector3 pos;
				float size;
							
				Gizmos.color = new Color(1f,1f,1f,0.1f);
					
				for(i=0;i < particlesPositions.Count;i++){
			
					pos = particlesPositions[i];
					size = particlesSizes[i];
					if(size > gizmoThreshold){
						//Gizmos.DrawWireSphere(_transform.TransformPoint(pos),size * 0.3f * particlesScale);
						Gizmos.DrawWireCube(_transform.TransformPoint(pos),Vector3.one * size * 0.3f * particlesScale);
					}
				}
				
			}else{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(transform.position,_transform.localScale.x);
			}
		}
	}
	#endif
}
