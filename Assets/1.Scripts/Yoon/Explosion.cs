using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{

    public GameObject Effect;
    //private Transform tr;

    private int hitCount = 0;

    // Use this for initialization
    void Start()
    {
       //tr = GetComponent<Transform>();
    }

    void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.tag == "BULLET")
        {
            Destroy(coll.gameObject);

            if (++hitCount >= 5)
            {
                CollisionEffect();
            }
        }
    }

    void CollisionEffect()
    {
        Instantiate(Effect, transform.position, Quaternion.identity);

        Destroy(gameObject, 0.0f);
    }
}
