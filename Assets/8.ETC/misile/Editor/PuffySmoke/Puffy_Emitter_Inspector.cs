using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Puffy_Emitter))]
public class Puffy_Emitter_Inspector : Editor {
	
		
	public override void OnInspectorGUI(){
		Puffy_Emitter myTarget = (Puffy_Emitter) target;
		
		myTarget.useThread = EditorGUILayout.Toggle("Use threads",myTarget.useThread);
		
		myTarget.autoAssign = EditorGUILayout.Toggle("Assign to first renderer",myTarget.autoAssign);
		if(myTarget.autoAssign){
			
			//Puffy_Renderer[] renderers = FindObjectsOfType(typeof(Puffy_Renderer)) as Puffy_Renderer[];
			
			//myTarget.autoRendererName = EditorGUILayout.TextField("Auto assign to",myTarget.autoRendererName);
		}else{
			
			myTarget.puffyRenderer = EditorGUILayout.ObjectField("Renderer",myTarget.puffyRenderer,typeof(Puffy_Renderer),true) as Puffy_Renderer;
			
		}
		
		myTarget.freezed = EditorGUILayout.Toggle("Freezed",myTarget.freezed);	
		
		// WIP
		// myTarget.useSpacePartitionning = EditorGUILayout.Toggle("Advanced Volumes",myTarget.useSpacePartitionning);
		
		EditorGUILayout.Separator();
		
		myTarget.autoResize = EditorGUILayout.Toggle("Unlimited particles",myTarget.autoResize);
		if(!myTarget.autoResize){
			myTarget.allowedParticleCount = (int)EditorGUILayout.Slider("Max particles count",myTarget.allowedParticleCount,10,100000);
		}
		myTarget.warmUp = EditorGUILayout.Toggle("Warm Up",myTarget.warmUp);	
		if(myTarget.warmUp){
			myTarget.warmUpCount = (int)EditorGUILayout.Slider("Warm Up Count",myTarget.warmUpCount,0,16000);
		}
		EditorGUILayout.Separator();
		
		myTarget.autoEmit = EditorGUILayout.Toggle("Auto emit",myTarget.autoEmit);
		
		if(myTarget.autoEmit){
			myTarget.spawnRate = EditorGUILayout.Slider("Particles/second",myTarget.spawnRate,1,5000);	
		}
		
		myTarget.subParticlesCount = (int)EditorGUILayout.Slider("Sub particles",myTarget.subParticlesCount,0,20);	
		if(myTarget.subParticlesCount>0){
			myTarget.subParticlesRatio = EditorGUILayout.Slider("Sub lifetime factor",myTarget.subParticlesRatio,0.0f,1f);
			myTarget.debugIntermediate = EditorGUILayout.Toggle("Debug sub particles",myTarget.debugIntermediate);
		}
		EditorGUILayout.Separator();
		
		myTarget.maxParticlesDistance = EditorGUILayout.Slider("Max gap (0=OFF)",myTarget.maxParticlesDistance,0.0f,10f);
	
		
		EditorGUILayout.Separator();
		
		myTarget.positionVariation = EditorGUILayout.Vector3Field("Position Variation",myTarget.positionVariation);
		
		EditorGUILayout.Separator();
		
		myTarget.startDirection = EditorGUILayout.Vector3Field("Direction",myTarget.startDirection);
		myTarget.startDirectionVariation = EditorGUILayout.Vector3Field("Direction Variation",myTarget.startDirectionVariation);
		
		EditorGUILayout.Separator();
		
		myTarget.lifeTime = Mathf.Max (0,EditorGUILayout.FloatField("Life time",myTarget.lifeTime));
		myTarget.lifeTimeVariation = Mathf.Min (myTarget.lifeTime , Mathf.Max (0,EditorGUILayout.FloatField("Life time variation (-/+)",myTarget.lifeTimeVariation)));
		
		myTarget.startSpeed = Mathf.Max (0,EditorGUILayout.FloatField("Start speed",myTarget.startSpeed));
		myTarget.startSpeedVariation = Mathf.Min (myTarget.startSpeed , Mathf.Max (0,EditorGUILayout.FloatField("Start speed variation (-/+)",myTarget.startSpeedVariation)));
		
		
		EditorGUILayout.Separator();
		
		myTarget.startSize = Mathf.Max (0,EditorGUILayout.FloatField("Start size",myTarget.startSize));
		myTarget.startSizeVariation = Mathf.Min (myTarget.startSize , Mathf.Max (0,EditorGUILayout.FloatField("Start size variation (-/+)",myTarget.startSizeVariation)));
		
		myTarget.endSize = Mathf.Max (0,EditorGUILayout.FloatField("End size",myTarget.endSize));
		myTarget.endSizeVariation = Mathf.Min (myTarget.endSize , Mathf.Max (0,EditorGUILayout.FloatField("End size variation (-/+)",myTarget.endSizeVariation)));
		
		EditorGUILayout.Separator();
		
		myTarget.colorMode = (Puffy_Emitter.colorModes)EditorGUILayout.EnumPopup("Color source",myTarget.colorMode);
		
		switch(myTarget.colorMode){
			case Puffy_Emitter.colorModes.Basic:
				myTarget.startColor = EditorGUILayout.ColorField("Start color", myTarget.startColor);
				myTarget.startColorVariation = EditorGUILayout.ColorField("Start color variation (-/+)", myTarget.startColorVariation);
						
				myTarget.endColor = EditorGUILayout.ColorField("End color", myTarget.endColor);
				myTarget.endColorVariation = EditorGUILayout.ColorField("End color variation (-/+)", myTarget.endColorVariation);
			
				if(myTarget.colorGradient != null) myTarget.colorGradient.enabled = false;
			break;
			
			case Puffy_Emitter.colorModes.Gradient:
				if(myTarget.gameObject.GetComponent<Puffy_Gradient>() == null){
					myTarget.colorGradient = (myTarget.gameObject.AddComponent<Puffy_Gradient>() as Puffy_Gradient);
				}
				if(myTarget.colorGradient != null) myTarget.colorGradient.enabled = true;
			break;
			
			case Puffy_Emitter.colorModes.Mesh:
				if(myTarget.colorGradient != null) myTarget.colorGradient.enabled = false;
			break;
		}
		
		myTarget.useLuminosity = EditorGUILayout.Toggle("Use Luminosity",myTarget.useLuminosity);
		
		if(myTarget.useLuminosity){
			myTarget.luminosityEndTime = EditorGUILayout.Slider("Luminosity life time",myTarget.luminosityEndTime,0.001f,myTarget.lifeTime + myTarget.lifeTimeVariation);
			myTarget.luminosityCurve = EditorGUILayout.CurveField("Luminosity",myTarget.luminosityCurve);
		}

		if(GUI.changed){
			EditorUtility.SetDirty(target);
		}
	}
}
