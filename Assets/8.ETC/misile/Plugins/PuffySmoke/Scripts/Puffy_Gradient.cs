using UnityEngine;
using System.Collections;

public class Puffy_Gradient: MonoBehaviour  {
	
	
	public float startOffset = 0f;
	public float duration = 1f;
	
	public Gradient gradient;
	
	[Range(0,100)]
	public int lookupPrecision = 0;
	private int previousPrecision = 0;
	
	private Color[] lookup;
	
	void Start () {
		UpdateLookup();
		Puffy_ParticleSpawner ps = this.GetComponent<Puffy_ParticleSpawner>();
		if(ps) ps.colorGradient = this;
	}
	
	void OnValidate(){
		UpdateLookup();
		
	}
	
	private void UpdateLookup(){
		if(lookupPrecision > 0){
			lookup = new Color[lookupPrecision];
			for(int i=0;i<lookupPrecision;i++){
				lookup[i] = gradient.Evaluate((float)i/(lookupPrecision-1));
			}
		}
		previousPrecision = lookupPrecision;
	}
	
	public Color Evaluate(float time){
		
		if(duration != 0){
			time = Mathf.Min (1f,(time - startOffset)/duration);
		}else{
			time = 0;
		}
		
		if(lookupPrecision == 0){
			return gradient.Evaluate(time);
		}else{
			if(lookupPrecision != previousPrecision) UpdateLookup();
			
			int index = Mathf.Max(0,Mathf.Min (lookupPrecision-1,Mathf.FloorToInt(lookupPrecision * time) ));
			return lookup[index];
		}
	}
	
}
