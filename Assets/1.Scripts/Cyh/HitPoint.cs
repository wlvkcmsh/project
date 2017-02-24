using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class HitPoint : MonoBehaviour {
	public float flashSpeed = 5f;
	public Color flashColour = new Color(1f,0f,0f,0.1f);
	public Image damageImage;
	public bool damaged = false;


	void Start () {
	
	}
	

	void Update ()
	{
		if (damaged)
		{
			damageImage.color = flashColour;
		}

		else
		{
			damageImage.color = Color.Lerp (damageImage.color, Color.clear, flashSpeed*Time.deltaTime);
		}

	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Missile")
		{
			damaged = true;
			Debug.Log ("dfa");
		} 
	}

	void OnTriggerExit(Collider other)
	{
		damaged = false;
	}


}
