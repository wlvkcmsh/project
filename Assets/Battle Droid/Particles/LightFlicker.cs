using UnityEngine;
using System.Collections;

public class LightFlicker : MonoBehaviour {
	private Light LightWeapon;
	private int control = 0;
	private int cont;

	void Start () 
	{
		LightWeapon = this.GetComponentInChildren<Light>();
	}
	


	void FixedUpdate () 
	{
		cont++;
		if (cont == 1)
		{
		if (control == 0)
		{
		
		LightWeapon.intensity = 0;
		control = 1;
		}
		}
		if (cont == 5)
		{
		if (control == 1)
		{
			
			LightWeapon.intensity = 10f;
			control = 0;
		}
		}
		if (cont == 6)
		{
			cont = 0;
		}


	}

}
