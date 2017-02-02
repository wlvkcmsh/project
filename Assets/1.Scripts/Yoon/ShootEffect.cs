using UnityEngine;
using System.Collections;

public class ShootEffect : MonoBehaviour {

    float removeTime;
    float durableTime;
    
    void Start()
    {
        removeTime = 2f;
    }

	void Update () {
        durableTime += Time.deltaTime;
        if(durableTime>=removeTime)
        {
            Destroy(gameObject);
        }
	}
}
