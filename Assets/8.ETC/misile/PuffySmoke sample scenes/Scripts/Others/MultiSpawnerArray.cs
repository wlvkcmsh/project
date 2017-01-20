using UnityEngine;
using System.Collections;

public class MultiSpawnerArray : Puffy_MultiSpawner {
	
	public int countX = 5;
	public int countZ = 5;
	
	void Start () {
		
		int x,z;
		
		float offsetX = countX * -0.5f;
		float offsetZ = countZ * -0.5f;
		
		for(x = 0;x < countX; x++){
			for(z = 0;z < countZ; z++){
				// create particles spawners and parent them to this object, so they will follow its orientation
				CreateSpawner( new Vector3(x + offsetX,0,z + offsetZ) , Vector3.up).gameObject.transform.parent = transform;
			}
		}
	}
	
	
	
}
