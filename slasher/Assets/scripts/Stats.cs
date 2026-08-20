using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    
    [Header("Stats")]
    public float health;
    public float stamina;
    public float speed;
    public float attack;
    public float luck;
    public int freePoint;

    private void Start()
    {

        health = 100f;
        stamina = 100f;
        speed = 5f;
        attack = 10f;
        luck = 1f;
    }

    public void HealthAdd(){
        if(freePoint > 0)
        {
            health += 50;
            freePoint -= 1;
        }
        
    }
    public void StaminaAdd()
    {
        if(freePoint > 0)
        {
            stamina += 25;
            freePoint -= 1;
        }
        
    }
    public void SpeedAdd()
    {
        if (freePoint > 0)
        {
            speed += 1;
            freePoint -= 1;
        }
        
    }
    public void AttackAdd()
    {
        if (freePoint > 0)
        {
            attack += 10;
            freePoint -= 1;
        }
        
    }
    public void LuckAdd()
    {
        if(freePoint > 0)
        {
            luck += 1;
            freePoint -= 1;
        }
        
    }
}
