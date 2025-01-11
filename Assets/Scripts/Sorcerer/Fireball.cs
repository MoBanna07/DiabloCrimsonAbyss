using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    private PlayerController sorcerer;

    void Start()
    {
        // Cache the player reference for efficiency
        sorcerer = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            MinionBehavior enemyController = other.GetComponent<MinionBehavior>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(5);
                if (enemyController.health <= 0)
                {
                    sorcerer.GainXP(10);
                }
            }
        }
        else if (other.CompareTag("Demon"))
        {
            Demon enemyController = other.GetComponent<Demon>();
            if (enemyController != null)
            {
                enemyController.isHit(5);
                if (enemyController.health <= 0)
                {
                    sorcerer.GainXP(30);
                }
            }
        }
    }
}
