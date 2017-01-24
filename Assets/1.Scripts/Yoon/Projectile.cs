using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{

    float speed = 10;
    float timer = 0.0f;
    float lifeTime = 3.0f;

    public void setSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        transform.Translate(Vector3.back * Time.deltaTime * speed);

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            timer = 0.0f;
            Destroy(gameObject);
        }

    }
}
