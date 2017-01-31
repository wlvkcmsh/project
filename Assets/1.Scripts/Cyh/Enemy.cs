using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{

    
    public int currentHealth ;

    public void Damage(int damageAmount)
    {
        
        currentHealth -= damageAmount;

       
        if (currentHealth <= 0)
        {
          
            gameObject.SetActive(false);
        }
    }
}