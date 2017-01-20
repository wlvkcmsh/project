using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Puffy_MultiSpawner))]
public class Puffy_MultiSpawner_Inspector : Editor {

	public override void OnInspectorGUI(){
		serializedObject.Update ();
		
		Puffy_MultiSpawner myTarget = (Puffy_MultiSpawner) target;
		
		myTarget.emitter = EditorGUILayout.ObjectField ("Puffy_Emitter", myTarget.emitter, typeof(Puffy_Emitter), true) as Puffy_Emitter;

		myTarget.prefab = EditorGUILayout.ObjectField ("Prefab", myTarget.prefab, typeof(GameObject), true) as GameObject;

		for(int i=0; i<myTarget.spawnerList.Count; i++){
			if(myTarget.spawnerList[i] == null){
				myTarget.spawnerList.RemoveAt(i);
				i--;
			}else{
				myTarget.spawnerList[i].emitter = myTarget.emitter;
			}
		}
		
		if(GUILayout.Button("Add Spawn Point"))
        {
			Puffy_ParticleSpawner sp = myTarget.CreateSpawner(myTarget.transform.position);
			sp.name = sp.name+" "+myTarget.spawnerList.Count;
			//sp.name = "Puffy Particles Spawner "+myTarget.spawnerList.Count;
			//sp.transform.parent = myTarget.transform;
			
			Selection.activeGameObject = sp.gameObject;
        }
		
//		EditorGUIUtility.LookLikeInspector ();
		SerializedProperty tps = serializedObject.FindProperty ("spawnerList");
		EditorGUI.BeginChangeCheck ();
		EditorGUILayout.PropertyField (tps, true);
		if (EditorGUI.EndChangeCheck ())
			serializedObject.ApplyModifiedProperties ();
		EditorGUIUtility.LookLikeControls ();
		
		if (GUI.changed) {
			EditorUtility.SetDirty (target);
		}
		 
		
	}
}
