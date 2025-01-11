using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inferno : MonoBehaviour
{
    private PlayerController sorcerer;
    private float damageTimer = 0f;

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
                enemyController.TakeDamage(10);

            }
        }
        else if (other.CompareTag("Demon"))
        {
            Demon demonController = other.GetComponent<Demon>();
            if (demonController != null)
            {
                demonController.isHit(10);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log("entered stay");

        // Increment the timer
        damageTimer += Time.deltaTime;
        Debug.Log(damageTimer);

        // Apply damage every 1 second
        if (damageTimer >= 1f)
        {
            if (other.CompareTag("Enemy"))
            {
                MinionBehavior enemyController = other.GetComponent<MinionBehavior>();
                if (enemyController != null)
                {
                    enemyController.TakeDamage(2);
                    damageTimer = 0f;
                }
            }
            else if (other.CompareTag("Demon"))
            {
                Demon demonController = other.GetComponent<Demon>();
                if (demonController != null)
                {
                    demonController.isHit(2);
                    damageTimer = 0f;
                }
            }

        }
    }
}

