using UnityEngine;
using System.Collections;

public class GPURenderTime : MonoBehaviour {

	private DebugTimer gpuTime = new DebugTimer();

	void OnPreRender(){
		gpuTime.Start();
	}
	
	void OnPostRender(){
		gpuTime.Stop();
	}
	
	void OnGUI() {
		GUILayout.Space(15);
		GUILayout.Label ("GPU : " + gpuTime.Average().ToString("f2")+"ms");
	}
	
}
