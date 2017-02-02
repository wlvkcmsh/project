using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{

    public GameObject shootEffect;
    public int currentHealth ;

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