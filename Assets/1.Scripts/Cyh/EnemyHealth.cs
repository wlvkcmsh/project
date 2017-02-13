using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour {


    public int currentHealth;
    public GameObject shootEffect;


    public void Damage(int damageAmount)
    {

        currentHealth -= damageAmount;


        if (currentHealth <= 0)
        {
            Instantiate(shootEffect, gameObject.transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }
}
