using UnityEngine;
using System.Collections;

public class StartShooting : MonoBehaviour {
	public GameObject r1laser1;
	public GameObject r1laser2;
	public GameObject r1machinegun;
	public AudioSource audio1;
	public AudioSource audio2;

	public AudioSource tras1;
	public AudioSource tras2;
	public AudioSource tras3;
	public AudioSource tras4;

	public AudioSource step;
	public AudioSource turn;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0))  //0 = Mouse Left  1 = Mouse Right
		{
			r1laser1.SetActive(true);
			r1laser2.SetActive(true);
			r1machinegun.SetActive(true);
			audio1.Play();
			audio2.Play();


		}
		if (Input.GetMouseButtonDown(1))  //0 = Mouse Left  1 = Mouse Right
		{
			r1laser1.SetActive(false);
			r1laser2.SetActive(false);
			r1machinegun.SetActive(false);
			audio1.Stop();
			audio2.Stop();
			
			
		}


		if (Input.GetKeyDown(KeyCode.Keypad1))  //0 = Mouse Left  1 = Mouse Right
		{
			//r1laser1.SetActive(true);
			//r1laser2.SetActive(true);
			//r1mitraglia.SetActive(true);
			//audio1.Play();
			//audio2.Play();
			
			
		}


		if (Input.GetKeyDown(KeyCode.Keypad1))
		    {
			tras1.Play();
			}

		if (Input.GetKeyDown(KeyCode.Keypad2))
		{
			tras2.Play();
		}

		if (Input.GetKeyDown(KeyCode.Keypad3))
		{
			tras3.Play();
		}
		if (Input.GetKeyDown(KeyCode.Keypad4))
		{
			tras4.Play();
		}

		if (Input.GetKeyDown(KeyCode.T))
		{
			turn.Play();
		}

		if (Input.GetKeyDown(KeyCode.W))
		{
			step.Play();
		}


	}
}
