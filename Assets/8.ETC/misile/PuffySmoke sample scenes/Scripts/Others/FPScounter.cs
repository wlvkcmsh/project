using UnityEngine;
using System.Collections;

public class FPScounter : MonoBehaviour {
	
	int frames = 0;
	double lastTime = 0;
	double elapsed = 0;
	string str = "";
		
	public float updateInterval = 0.25f;
	
	// Use this for initialization
	void Start () {
		Application.targetFrameRate = 60;
		lastTime = Time.realtimeSinceStartup;
	}
	
	// Update is called once per frame
	void Update () {
		++frames;
	    
		elapsed = Time.realtimeSinceStartup - lastTime;
		
	    // Interval ended - update GUI text and start new interval
	    if( elapsed >= updateInterval)
	    {
			str = "" + (frames/elapsed).ToString("f2")+" FPS";
			lastTime = Time.realtimeSinceStartup;
			frames = 0;
	    }
	}
	
	void OnGUI() {
 		GUILayout.Label(str);
	}
}
