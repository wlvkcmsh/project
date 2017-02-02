using UnityEngine;
using System.Collections;

public class Shell : MonoBehaviour
{
    public Rigidbody myRigidbody;
    public float forceMin;
    public float forceMax;

    float lifeTime;
    float fadeTime;

    void Start()
    {
        float force = Random.Range(forceMin, forceMax);
        myRigidbody.AddForce(transform.right * force);
        myRigidbody.AddTorque(Random.insideUnitSphere * force);

        lifeTime = 2;
        fadeTime = 2;

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifeTime);

        float percent = 0;
        float fadeSpeed = 1 / fadeTime;
        Material mater = GetComponent<Renderer>().material;
        Color initColour = mater.color;

        while (percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;
            mater.color = Color.Lerp(initColour, Color.clear, percent);
            yield return null;
        }

        Destroy(gameObject);
    }
}
