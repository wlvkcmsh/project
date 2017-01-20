using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MissileLauncher : Puffy_MultiSpawner {
	
	//private List<HomingMissile> missiles = new List<HomingMissile>();
	private Transform missilesContainer;
	
	private Transform target; // object to follow

	public int launchCount = 1;
	public float missileAccuracy = 0.5f;
	public float missileCrazyness = 0.2f;
    public float shootTime;

    public AudioClip fireSfx; //총알 발사 사운드
    public AudioSource source = null; //컴포넌트 저장
    public GameObject leftfirepos;
    public GameObject rightfirepos;
    private float missileculTime = 0.0f;
    public enemy enemy;
    void Start(){
		// missiles will be parented to this gameObject, to prevent long list of items in the editor hierachy view
		missilesContainer = new GameObject("Missiles Container of ["+name+"]").transform;
        target = GameObject.FindWithTag("Player").GetComponent<Transform>();
    }
	
	void Update () {
		
		// synchronize freeze state with puffy smoke freeze
		HomingMissile.globalFreeze = Puffy_Emitter.globalFreeze;

        if (enemy.monsterState == enemy.MonsterState.attack)
        {
            missileculTime += Time.deltaTime;

            if (missileculTime >= shootTime)
            {
                // get worldspace mouse pointer position
                Vector3 pos = leftfirepos.transform.position;
                Vector3 pos2 = rightfirepos.transform.position;
                source.PlayOneShot(fireSfx, 0.9f); //사운드 발생
                source.PlayOneShot(fireSfx, 0.9f); //사운드 발생
                source.PlayOneShot(fireSfx, 0.9f); //사운드 발생
                                                   // launch missiles
                Fire(launchCount, pos);
                Fire(launchCount, pos2);

            }


        }

    }
	
	public void Fire(int count,Vector3 startPosition){
		HomingMissile missileScript = null;
		Vector3 position;
        missileculTime = 0.0f;

        for (int m = 0; m < count; m++){

			position = startPosition;
			// randomly offset the missile start position if more than one missile are launched at the same time
			if(count > 1) position += Random.onUnitSphere;

            // get the next available missile (or instantiate a new one if none is free to reuse)
            Puffy_ParticleSpawner p = CreateSpawner(position, Vector3.Normalize(target.transform.position - position));
			p.transform.parent = missilesContainer;

			missileScript = p.GetComponent<HomingMissile>();

			if(missileScript == null) missileScript = p.gameObject.AddComponent<HomingMissile>();

			// update turning and craziness values for this missile
			missileScript.angularThreshold = missileAccuracy;
			missileScript.craziness = missileCrazyness;
			
			// spawn the missile and initialize it with its new start position, direction, target and speed
			missileScript.Spawn(position, Vector3.Normalize(target.transform.position - position), target, missileScript.speed);
		}
	}
}
