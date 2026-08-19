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
        speed = 10f;
        attack = 10f;
        luck = 1f;
    }

    public void HealthAdd(){health += 50;}
    public void StaminaAdd(){health += 25;}
    public void SpeedAdd(){health += 1;}
    public void AttackAdd(){health += 10;}
    public void LuckAdd(){health += 1;}
}
