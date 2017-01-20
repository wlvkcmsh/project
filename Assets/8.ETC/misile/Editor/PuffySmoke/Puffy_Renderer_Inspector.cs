using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class FoldRestorer
{
	private static Dictionary<int, bool> folds = new Dictionary<int,bool> ();
 
	public static bool GetFold (int hash)
	{
		if (folds.ContainsKey (hash) == false) {
			folds [hash] = false;
			return false;
		}
		return folds [hash];
	}
 
	public static void SetFold (int hash, bool value)
	{
		folds [hash] = value;
	}
}

[CustomEditor(typeof(Puffy_Renderer))]
public class Puffy_Renderer_Inspector : Editor
{
	
	public override void OnInspectorGUI ()
	{
		serializedObject.Update ();
		
		Puffy_Renderer myTarget = (Puffy_Renderer)target;
		
		myTarget.perf_foldout = EditorGUILayout.Foldout (myTarget.perf_foldout, "Performances");
		
		if (myTarget.perf_foldout) {
			//myTarget.render = EditorGUILayout.Toggle("Render",myTarget.render);

			myTarget.PassMode = (Puffy_Renderer.PassModeType)EditorGUILayout.EnumPopup ("Pass Mode", myTarget.PassMode);
			if (myTarget.PassMode == Puffy_Renderer.PassModeType.Auto) {
				myTarget.updateThreshold = (float)EditorGUILayout.Slider ("Update threshold", myTarget.updateThreshold, 0f, 0.1f);
			}

			myTarget.radixSort = EditorGUILayout.Toggle ("Faster depth sorting", myTarget.radixSort);

			myTarget.useThread = EditorGUILayout.Toggle ("Use Threads", myTarget.useThread);
			if (myTarget.useThread) {
				myTarget.coresSetup = (Puffy_Renderer.coreslevels)EditorGUILayout.EnumPopup ("Cores Setup", myTarget.coresSetup);
			}
			EditorGUILayout.Space ();
			myTarget.meshSetup = (Puffy_Renderer.meshlevels)EditorGUILayout.EnumPopup ("Particles per mesh", myTarget.meshSetup);
			myTarget.subMeshCount = EditorGUILayout.IntSlider("Sub mesh counts",myTarget.subMeshCount,1,8);
			myTarget.warmUp = EditorGUILayout.Toggle("Warm Up",myTarget.warmUp);
			if(myTarget.warmUp){
				myTarget.warmUpCount = (int)EditorGUILayout.Slider("Warm Up Mesh Count",myTarget.warmUpCount,1,32);
			}
			EditorGUILayout.Space ();
			
			myTarget.debug = EditorGUILayout.Toggle ("Show Debug", myTarget.debug);
			if (myTarget.debug) {
				myTarget.debugMode = (Puffy_Renderer.debugModes)EditorGUILayout.EnumPopup ("Debug Mode", myTarget.debugMode);
				
			}
		}
		
		
		myTarget.aspect_foldout = EditorGUILayout.Foldout (myTarget.aspect_foldout, "Aspect");
		if (myTarget.aspect_foldout) {
			myTarget._light = EditorGUILayout.ObjectField ("Light", myTarget._light, typeof(Light), true) as Light;
			
			myTarget.useAmbientColor = EditorGUILayout.Toggle ("Use Ambient Color", myTarget.useAmbientColor);
			if (myTarget.useAmbientColor) {
				myTarget.cameraBackgroundAsAmbientColor = EditorGUILayout.Toggle ("Camera BG as Ambient Color", myTarget.cameraBackgroundAsAmbientColor);
				
				myTarget.ambientIntensity = EditorGUILayout.Slider ("Ambient intensity", myTarget.ambientIntensity, 0.0f, 2f);
			}
			
			EditorGUILayout.Space ();
			
			myTarget.particlesMaterial = EditorGUILayout.ObjectField ("Material", myTarget.particlesMaterial, typeof(Material), false) as Material;
			myTarget.TextureColCount = EditorGUILayout.IntField ("Texture Col Count", myTarget.TextureColCount);
			myTarget.TextureRowCount = EditorGUILayout.IntField ("Texture Row Count", myTarget.TextureRowCount);
			
			//EditorGUILayout.Space ();
			//myTarget.detailsScaling = EditorGUILayout.Slider ("Details scaling", myTarget.detailsScaling, 0.01f, 2f);
		}
		
		myTarget.visibility_foldout = EditorGUILayout.Foldout (myTarget.visibility_foldout, "Visibility");
		
		if (myTarget.visibility_foldout) {
			
			myTarget.AutoLOD = EditorGUILayout.Toggle ("Auto LOD", myTarget.AutoLOD);
			if(myTarget.AutoLOD) myTarget.LODstartDistance = EditorGUILayout.IntField ("LOD start distance", myTarget.LODstartDistance);
			
			myTarget.MaxRenderDistance = EditorGUILayout.IntField ("Max render distance", myTarget.MaxRenderDistance);
							
			myTarget.ScreensizeClipping = EditorGUILayout.Slider ("Screensize Near clipping", myTarget.ScreensizeClipping, 0f, 4f);
			myTarget.ScreensizeClippingFade = EditorGUILayout.Slider ("Near clipping fade range", myTarget.ScreensizeClippingFade, 0f, 1f);

		}
		
//		EditorGUIUtility.LookLikeInspector ();
		SerializedProperty tps = serializedObject.FindProperty ("Emitters");
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