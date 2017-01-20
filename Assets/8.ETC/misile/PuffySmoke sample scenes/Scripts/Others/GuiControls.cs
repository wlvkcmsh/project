using UnityEngine;
using System.Collections;

public class GuiControls : MonoBehaviour {
	
	public static bool showGUI = true;
	
	public Light mainLight;
	private bool freezeNext = false;
	
	void Start(){
		if(mainLight == null){
			Light[] sceneLights = FindObjectsOfType(typeof(Light)) as Light[];
			foreach(Light l in sceneLights){
				if(l.type == LightType.Directional){
					mainLight = l;
					break;
				}
			}
		}
	}
	
	void OnGUI(){
		if(showGUI){
			int y=0;
			
			Puffy_Emitter.globalFreeze = GUI.Toggle(new Rect(350,y,120,20),Puffy_Emitter.globalFreeze ,"Freeze particles");
			
			GUI.Label(new Rect(470,y,100,20),"TimeScale");
			Time.timeScale = GUI.HorizontalSlider(new Rect(540,y+5,300,20),Time.timeScale ,0f,2f);
		
			if(mainLight){
				GUI.Label(new Rect(130,y,100,20),"Light intensity");
				mainLight.intensity = GUI.HorizontalSlider(new Rect(220,y+5,100,20),mainLight.intensity,0f,2.5f);
			}
			
			string str = "";
			str += "U / I : rotate light\n";
			str += "P : toggle freeze\n";
			str += "O : step freeze\n";
			str += "M : timeScale = 0.001\n";
			str += "L : timeScale = 1\n";
			str += "D : toggle debug display\n";
			str += "H : toggle GUI";
			
			GUI.Label (new Rect(Screen.width - 205,25,200,120),str);
			
			
			if(GUI.Button(new Rect(Screen.width - 105,Screen.height - 60,100,25),"Next Demo >>")){
				Application.LoadLevel((Application.loadedLevel+1) % Application.levelCount);
			}
			
		}
	}
	
	void Update(){
		// toggle pause
		if(Input.GetKeyDown(KeyCode.P) || freezeNext){
			Puffy_Emitter.globalFreeze = !Puffy_Emitter.globalFreeze;
			freezeNext = false;
		}
		
		// pause step one time
		if(Input.GetKeyDown(KeyCode.O)){
			Puffy_Emitter.globalFreeze = false;
			freezeNext = true;
		}
		
		// minimum time scale
		if(Input.GetKeyDown(KeyCode.M)){
			Time.timeScale = 0.001f;
		}
		
		// normal time scale
		if(Input.GetKeyDown(KeyCode.L)){
			Time.timeScale = 1f;
		}
		
		// toggle debug display
		if(Input.GetKeyDown(KeyCode.D)){
			Puffy_Renderer.ToggleDebug();
		}
		
		// toggle gui
		if(Input.GetKeyDown(KeyCode.H)){
			showGUI = !showGUI;
		}
		
		// rotate light
		if(mainLight){
			if(Input.GetKey(KeyCode.U)){
				mainLight.transform.Rotate(0,-1,0);
			}
			if(Input.GetKey(KeyCode.I)){
				mainLight.transform.Rotate(0,1,0);
			}
		}
		
	}
}
