using UnityEngine;
using System.Collections;

public class MissilesGuiControls : MonoBehaviour {

	public MissileLauncher missileLauncher;
	
	void Start(){
		if(!missileLauncher) missileLauncher = GetComponent<MissileLauncher>();
	}
	
	
	void OnGUI(){
		if(GuiControls.showGUI){
			if(missileLauncher){
				int x=Screen.width - 205;
				
				string str = "Homing Missiles controls :\n";
				str += "LMB : launch missiles\n";
				str += "RMB : stick target to mouse\n";
				str += "Mouse : rotate camera\n";
				str += "Arrows : move camera\n";
				str += "Hold Shift : lock camera rotation\n";
				str += "Hold Ctrl : slow camera\n";
				str += "T : toggle target movements\n";
				
				GUI.Label (new Rect(x,25+130,200,130),str);
				
				GUI.Label(new Rect(x,295,120,20),"Launch count "+missileLauncher.launchCount);
				missileLauncher.launchCount = (int)GUI.HorizontalSlider(new Rect(x + 100,300,85,20),missileLauncher.launchCount,1,10);
				
				GUI.Label(new Rect(x,315,120,20),"Accuracy "+missileLauncher.missileAccuracy.ToString("f2"));
				missileLauncher.missileAccuracy = GUI.HorizontalSlider(new Rect(x + 100,320,85,20),missileLauncher.missileAccuracy,0.1f,10f);
				
				GUI.Label(new Rect(x,335,120,20),"Crazyness "+missileLauncher.missileCrazyness.ToString("f2"));
				missileLauncher.missileCrazyness = GUI.HorizontalSlider(new Rect(x + 100,340,85,20),missileLauncher.missileCrazyness,0.0f,2f);
			}
		}
	}
}
