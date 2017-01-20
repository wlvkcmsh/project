using UnityEngine;
using System.Collections;
using UnityEditor;

public static class Puffy_AddPresetMenu {
	
	[MenuItem("GameObject/Create Other/Puffy Smoke/Renderer")]
	static void CreateRenderer(){
		GameObject go = new GameObject("Puffy Renderer");
		Puffy_Renderer renderer = go.AddComponent<Puffy_Renderer>() as Puffy_Renderer;
		renderer.enabled = true;
    }
	
	[MenuItem("GameObject/Create Other/Puffy Smoke/Simple Emitter")]
	static void CreateEmitter(){
		GameObject go = new GameObject("Puffy Emitter");
		Puffy_Emitter emitter = go.AddComponent<Puffy_Emitter>() as Puffy_Emitter;
		emitter.enabled = true;
    }

	[MenuItem("GameObject/Create Other/Puffy Smoke/Particle Multi Spawner")]
	static void CreateMultiSpawner(){
		GameObject go = new GameObject("Puffy MultiSpawner");
		Puffy_Emitter emitter = go.AddComponent<Puffy_Emitter>() as Puffy_Emitter;
		emitter.enabled = true;
		Puffy_MultiSpawner spawner = go.AddComponent<Puffy_MultiSpawner>() as Puffy_MultiSpawner;
		spawner.enabled = true;
		spawner.emitter = emitter;
    }
}
    
    