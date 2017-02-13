using UnityEngine;
using System.Collections;

public class Gun_test : MonoBehaviour {

    public Transform muzzle;
    public Projectile_test projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;

    float nextShotTime;

    public void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            nextShotTime = Time.time + msBetweenShots / 1000;
            Projectile_test newProjectole = Instantiate(projectile, muzzle.position, muzzle.rotation) as Projectile_test;
            newProjectole.SetSpeed(muzzleVelocity);
        }
    }
	
	void Start () {
	
	}
	
	
	void Update () {
	
	}
}
